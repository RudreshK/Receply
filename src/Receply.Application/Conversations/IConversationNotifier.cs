using Receply.Application.Conversations.Queries.GetConversationMessages;

namespace Receply.Application.Conversations;

/// <summary>Pushes newly saved conversation messages to connected dashboards in real time.</summary>
public interface IConversationNotifier
{
    Task MessageAddedAsync(Guid tenantId, Guid conversationId, ConversationMessageDto message, CancellationToken cancellationToken = default);
}

/// <summary>Default for hosts with no real-time channel (e.g. Workers); the Api host replaces it.</summary>
public class NullConversationNotifier : IConversationNotifier
{
    public Task MessageAddedAsync(Guid tenantId, Guid conversationId, ConversationMessageDto message, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
