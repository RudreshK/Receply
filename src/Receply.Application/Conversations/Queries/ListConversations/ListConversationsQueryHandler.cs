using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Conversations.Queries.ListConversations;

public class ListConversationsQueryHandler(IApplicationDbContext db) : IRequestHandler<ListConversationsQuery, List<ConversationListItemDto>>
{
    public async Task<List<ConversationListItemDto>> Handle(ListConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await (
            from c in db.Conversations
            join client in db.Clients on c.ClientId equals client.Id
            where c.TenantId == request.TenantId
            select new
            {
                c.Id,
                c.ClientId,
                c.Status,
                client.FullName,
                client.PhoneNumber,
                ClientCreatedOn = client.CreatedOn
            }).ToListAsync(cancellationToken);

        var conversationIds = conversations.Select(c => c.Id).ToList();

        // Grouped client-side rather than with a "top 1 per group" DB query - simpler to get right,
        // and message volume per tenant is low enough at this stage that it's not worth the risk of
        // an EF/Npgsql translation surprise on a window-function query.
        var lastMessageByConversation = (await db.Messages
                .Where(m => conversationIds.Contains(m.ConversationId))
                .OrderBy(m => m.SentAtUtc)
                .Select(m => new { m.ConversationId, m.Body, m.SentAtUtc })
                .ToListAsync(cancellationToken))
            .GroupBy(m => m.ConversationId)
            .ToDictionary(g => g.Key, g => g.Last());

        return conversations
            .Select(c =>
            {
                lastMessageByConversation.TryGetValue(c.Id, out var last);
                var displayPhone = FormatPhoneForDisplay(c.PhoneNumber);
                return new ConversationListItemDto(
                    c.Id,
                    c.ClientId,
                    c.FullName ?? displayPhone,
                    displayPhone,
                    c.Status,
                    last?.Body,
                    last?.SentAtUtc,
                    c.ClientCreatedOn);
            })
            .OrderByDescending(c => c.LastMessageAtUtc ?? c.ClientCreatedOnUtc)
            .ToList();
    }

    // WhatsApp/the DB store phone numbers without a leading "+" (that's the wire format the Cloud API
    // itself expects for outbound sends) - this formats a copy for display only, never touching the
    // stored value or anything sent back to WhatsApp.
    private static string FormatPhoneForDisplay(string phoneNumber) =>
        phoneNumber.StartsWith('+') ? phoneNumber : $"+{phoneNumber}";
}
