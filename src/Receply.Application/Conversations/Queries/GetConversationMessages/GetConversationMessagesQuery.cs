using MediatR;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Queries.GetConversationMessages;

public record ConversationMessageDto(Guid MessageId, MessageDirection Direction, MessageSender Sender, string Body, DateTimeOffset SentAtUtc);

/// <summary>The full transcript for one conversation, oldest first - the dashboard's chat panel.</summary>
public record GetConversationMessagesQuery(Guid TenantId, Guid ConversationId) : IRequest<List<ConversationMessageDto>>;
