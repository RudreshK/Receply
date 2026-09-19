using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Receply.Api.BackgroundProcessing;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations.Commands.GenerateAiReply;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;
using Receply.Domain.Crm;
using Receply.Infrastructure.Channels.WhatsApp;

namespace Receply.Api.Controllers;

[ApiController]
[Route("api/webhooks/whatsapp")]
public class WhatsAppWebhookController(
    IChannelProvider channelProvider,
    IOptions<WhatsAppOptions> whatsAppOptions,
    IApplicationDbContext db,
    IBackgroundTaskQueue taskQueue,
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

        var customer = await db.Customers.FirstOrDefaultAsync(
            c => c.TenantId == channelAccount.TenantId && c.PhoneNumber == inbound.CustomerPhoneNumber, cancellationToken);

        if (customer is null)
        {
            customer = Customer.Create(channelAccount.TenantId, inbound.CustomerPhoneNumber);
            db.Customers.Add(customer);
        }

        var conversation = await db.Conversations
            .Where(c => c.TenantId == channelAccount.TenantId && c.CustomerId == customer.Id && c.Status != HandoffStatus.Resolved)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (conversation is null)
        {
            conversation = Conversation.Start(channelAccount.TenantId, customer.Id, channelAccount.Id);
            db.Conversations.Add(conversation);
        }

        conversation.AddMessage(MessageDirection.Inbound, MessageSender.Customer, inbound.Body);

        await db.SaveChangesAsync(cancellationToken);

        return (channelAccount.TenantId, conversation.Id);
    }
}
