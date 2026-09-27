using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Receply.Api.BackgroundProcessing;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations.Commands.GenerateAiReply;
using Receply.Application.Conversations.Commands.SendWelcomeMessage;
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
                var (tenantId, conversationId, isNewConversation) = ingested.Value;

                // Queued in order - the queue is single-reader/FIFO, so the welcome always lands before the AI's reply.
                if (isNewConversation)
                {
                    taskQueue.QueueWorkItem((services, ct) =>
                        services.GetRequiredService<ISender>().Send(new SendWelcomeMessageCommand(tenantId, conversationId), ct));
                }

                taskQueue.QueueWorkItem((services, ct) =>
                    services.GetRequiredService<ISender>().Send(new GenerateAiReplyCommand(tenantId, conversationId), ct));
            }
        }

        // Meta requires a fast 200 OK; the AI reply itself runs on the background queue above.
        return Ok();
    }

    private async Task<(Guid TenantId, Guid ConversationId, bool IsNewConversation)?> IngestMessageAsync(
        InboundChannelMessage inbound, CancellationToken cancellationToken)
    {
        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(
            c => c.Type == ChannelType.WhatsApp && c.ExternalId == inbound.FromExternalId, cancellationToken);

        if (channelAccount is null)
        {
            logger.LogWarning("Received WhatsApp message for unknown phone_number_id {ExternalId}.", inbound.FromExternalId);
            return null;
        }

        var client = await db.Clients.FirstOrDefaultAsync(
            c => c.TenantId == channelAccount.TenantId && c.PhoneNumber == inbound.CustomerPhoneNumber, cancellationToken);

        if (client is null)
        {
            client = Client.Create(channelAccount.TenantId, inbound.CustomerPhoneNumber);
            db.Clients.Add(client);
        }

        var conversation = await db.Conversations
            .Where(c => c.TenantId == channelAccount.TenantId && c.ClientId == client.Id && c.Status != HandoffStatus.Resolved)
            .OrderByDescending(c => c.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var isNewConversation = conversation is null;
        if (conversation is null)
        {
            conversation = Conversation.Start(channelAccount.TenantId, client.Id, channelAccount.Id);
            db.Conversations.Add(conversation);
        }

        conversation.AddMessage(MessageDirection.Inbound, MessageSender.Client, inbound.Body);

        await db.SaveChangesAsync(cancellationToken);

        return (channelAccount.TenantId, conversation.Id, isNewConversation);
    }
}
