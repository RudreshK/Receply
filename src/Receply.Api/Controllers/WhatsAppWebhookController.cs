using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Receply.Api.BackgroundProcessing;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations;
using Receply.Application.Conversations.Commands.GenerateAiReply;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Application.Tenancy.Commands.GenerateStaffAiReply;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;
using Receply.Domain.Crm;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Channels.WhatsApp;

namespace Receply.Api.Controllers;

[ApiController]
[Route("api/webhooks/whatsapp")]
public class WhatsAppWebhookController(
    IChannelProvider channelProvider,
    IOptions<WhatsAppOptions> whatsAppOptions,
    IApplicationDbContext db,
    IBackgroundTaskQueue taskQueue,
    IConversationNotifier conversationNotifier,
    ILogger<WhatsAppWebhookController> logger) : ControllerBase
{
    // Meta calls this once, at setup time, to prove you control the endpoint.
    [HttpGet]
    public IActionResult VerifyHandshake(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge)
    {
        if (mode == "subscribe" && verifyToken == whatsAppOptions.Value.WebhookVerifyToken && challenge is not null)
            return Content(challenge, "text/plain");

        return Forbid();
    }

    // Meta calls this for every inbound message/status update.
    [HttpPost]
    public async Task<IActionResult> ReceiveMessage(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawPayload = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
        if (!channelProvider.VerifyWebhookSignature(rawPayload, signature))
        {
            logger.LogWarning("Rejected WhatsApp webhook with invalid signature.");
            return Unauthorized();
        }

        var inboundMessages = channelProvider.ParseInboundWebhook(rawPayload);

        foreach (var inbound in inboundMessages)
        {
            var ingested = await IngestMessageAsync(inbound, cancellationToken);
            if (ingested is not null)
            {
                var (tenantId, conversationId) = ingested.Value;
                taskQueue.QueueWorkItem((services, ct) =>
                    services.GetRequiredService<ISender>().Send(new GenerateAiReplyCommand(tenantId, conversationId), ct));
            }
        }

        await ProcessStatusUpdatesAsync(rawPayload, cancellationToken);

        // Meta requires a fast 200 OK; the AI reply itself runs on the background queue above.
        return Ok();
    }

    private async Task<(Guid TenantId, Guid ConversationId)?> IngestMessageAsync(InboundChannelMessage inbound, CancellationToken cancellationToken)
    {
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.Type == ChannelType.WhatsApp && c.ExternalId == inbound.FromExternalId, cancellationToken);

        if (channelAccount is null)
        {
            logger.LogWarning("Received WhatsApp message for unknown phone_number_id {ExternalId}.", inbound.FromExternalId);
            return null;
        }

        if (await TryHandleStaffMessageAsync(channelAccount.TenantId, inbound, cancellationToken))
            return null;

        if (inbound.ExternalMessageId is not null)
        {
            // Best-effort (never throws) - shows the customer a blue tick on their message and a
            // "typing…" indicator while we work out a reply below, so they don't wander off.
            await channelProvider.ShowTypingIndicatorAsync(channelAccount, inbound.ExternalMessageId, cancellationToken);
        }

        var client = await db.Clients.FirstOrDefaultAsync(
            c => c.TenantId == channelAccount.TenantId && c.PhoneNumber == inbound.CustomerPhoneNumber, cancellationToken);

        if (client is null)
        {
            client = Client.Create(channelAccount.TenantId, inbound.CustomerPhoneNumber, inbound.SenderDisplayName);
            db.Clients.Add(client);
        }
        else if (inbound.SenderDisplayName is not null)
        {
            client.UpdateProfile(inbound.SenderDisplayName, null);
        }

        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .Where(c => c.TenantId == channelAccount.TenantId && c.ClientId == client.Id && c.Status != HandoffStatus.Resolved)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            conversation = Conversation.Start(channelAccount.TenantId, client.Id, channelAccount.Id);
            db.Conversations.Add(conversation);
        }

        var message = conversation.AddMessage(MessageDirection.Inbound, MessageSender.Client, inbound.Body, inbound.ExternalMessageId);

        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            channelAccount.TenantId,
            conversation.Id,
            new ConversationMessageDto(message.Id, message.Direction, message.Sender, message.Body, message.SentAtUtc, message.DeliveryStatus),
            cancellationToken);

        return (channelAccount.TenantId, conversation.Id);
    }

    /// <summary>Delivery-status webhooks ("sent"/"delivered"/"read") for messages we previously sent -
    /// drives the dashboard's WhatsApp-style ticks.</summary>
    private async Task ProcessStatusUpdatesAsync(string rawPayload, CancellationToken cancellationToken)
    {
        var statusUpdates = channelProvider.ParseStatusUpdates(rawPayload);
        if (statusUpdates.Count == 0)
            return;

        foreach (var update in statusUpdates)
        {
            var status = MapDeliveryStatus(update.Status);
            if (status is null)
                continue;

            var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
                c => c.Type == ChannelType.WhatsApp && c.ExternalId == update.FromExternalId, cancellationToken);
            if (channelAccount is null)
                continue;

            var message = await db.Messages.FirstOrDefaultAsync(
                m => m.TenantId == channelAccount.TenantId && m.ExternalMessageId == update.ExternalMessageId, cancellationToken);
            if (message is null)
                continue; // e.g. a status update for a message sent before this feature shipped

            message.UpdateDeliveryStatus(status.Value);
            await db.SaveChangesAsync(cancellationToken);

            await conversationNotifier.MessageStatusChangedAsync(
                channelAccount.TenantId, message.ConversationId, message.Id, message.DeliveryStatus, cancellationToken);
        }
    }

    private static MessageDeliveryStatus? MapDeliveryStatus(string providerStatus) => providerStatus switch
    {
        "sent" => MessageDeliveryStatus.Sent,
        "delivered" => MessageDeliveryStatus.Delivered,
        "read" => MessageDeliveryStatus.Read,
        "failed" => MessageDeliveryStatus.Failed,
        _ => null
    };

    /// <summary>
    /// Owner/Admin staff get a separate, stateless admin-tools flow (holidays, own details) instead of
    /// being onboarded as a customer. Other staff roles fall through and are treated as customers.
    /// </summary>
    private async Task<bool> TryHandleStaffMessageAsync(Guid tenantId, InboundChannelMessage inbound, CancellationToken cancellationToken)
    {
        var staff = await db.Staff.FirstOrDefaultAsync(
            s => s.TenantId == tenantId && s.PhoneNumber == inbound.CustomerPhoneNumber && s.IsActive
                 && (s.Role == StaffRole.Owner || s.Role == StaffRole.Admin),
            cancellationToken);

        if (staff is null)
            return false;

        taskQueue.QueueWorkItem((services, ct) =>
            services.GetRequiredService<ISender>().Send(new GenerateStaffAiReplyCommand(tenantId, staff.Id, inbound.Body), ct));

        return true;
    }
}
