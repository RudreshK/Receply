using Microsoft.AspNetCore.SignalR;
using Receply.Api.Hubs;
using Receply.Application.Conversations;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Domain.Conversations;

namespace Receply.Api.Realtime;

public class SignalRConversationNotifier(IHubContext<ConversationsHub> hubContext, ILogger<SignalRConversationNotifier> logger)
    : IConversationNotifier
{
    public async Task MessageAddedAsync(Guid tenantId, Guid conversationId, ConversationMessageDto message, CancellationToken cancellationToken = default)
    {
        // A dashboard push is best-effort - the message is already saved, so a SignalR hiccup must
        // never fail the webhook or the AI reply that triggered it.
        try
        {
            await hubContext.Clients.Group(ConversationsHub.TenantGroup(tenantId))
                .SendAsync("messageAdded", new { conversationId, message }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push message {MessageId} to dashboards.", message.MessageId);
        }
    }

    public async Task StatusChangedAsync(Guid tenantId, Guid conversationId, HandoffStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.Group(ConversationsHub.TenantGroup(tenantId))
                .SendAsync("statusChanged", new { conversationId, status }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push status change for conversation {ConversationId} to dashboards.", conversationId);
        }
    }

    public async Task MessageStatusChangedAsync(Guid tenantId, Guid conversationId, Guid messageId, MessageDeliveryStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients.Group(ConversationsHub.TenantGroup(tenantId))
                .SendAsync("messageStatusChanged", new { conversationId, messageId, status }, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to push delivery status for message {MessageId} to dashboards.", messageId);
        }
    }
}
