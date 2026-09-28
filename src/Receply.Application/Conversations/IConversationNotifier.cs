using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations;

/// <summary>Pushes newly saved conversation messages and status changes to connected dashboards in real time.</summary>
public interface IConversationNotifier
{
    Task MessageAddedAsync(Guid tenantId, Guid conversationId, ConversationMessageDto message, CancellationToken cancellationToken = default);

    Task StatusChangedAsync(Guid tenantId, Guid conversationId, HandoffStatus status, CancellationToken cancellationToken = default);

    /// <summary>A message's delivery state changed (sent/delivered/read/failed) - the dashboard's WhatsApp-style ticks.</summary>
    Task MessageStatusChangedAsync(Guid tenantId, Guid conversationId, Guid messageId, MessageDeliveryStatus status, CancellationToken cancellationToken = default);
}

/// <summary>Default for hosts with no real-time channel (e.g. Workers); the Api host replaces it.</summary>
public class NullConversationNotifier : IConversationNotifier
{
    public Task MessageAddedAsync(Guid tenantId, Guid conversationId, ConversationMessageDto message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task StatusChangedAsync(Guid tenantId, Guid conversationId, HandoffStatus status, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task MessageStatusChangedAsync(Guid tenantId, Guid conversationId, Guid messageId, MessageDeliveryStatus status, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
