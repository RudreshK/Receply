using MediatR;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Queries.ListConversations;

public record ConversationListItemDto(
    Guid ConversationId,
    Guid ClientId,
    string ClientName,
    string ClientPhoneNumber,
    HandoffStatus Status,
    string? LastMessagePreview,
    DateTimeOffset? LastMessageAtUtc,
    DateTimeOffset ClientCreatedOnUtc);

/// <summary>Every conversation for the tenant, newest activity first - the dashboard's client/inbox list.</summary>
public record ListConversationsQuery(Guid TenantId) : IRequest<List<ConversationListItemDto>>;
