using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Conversations.Queries.GetConversationMessages;

public class GetConversationMessagesQueryHandler(IApplicationDbContext db) : IRequestHandler<GetConversationMessagesQuery, List<ConversationMessageDto>>
{
    public async Task<List<ConversationMessageDto>> Handle(GetConversationMessagesQuery request, CancellationToken cancellationToken)
    {
        var exists = await db.Conversations.AnyAsync(
            c => c.Id == request.ConversationId && c.TenantId == request.TenantId, cancellationToken);

        if (!exists)
            throw new KeyNotFoundException($"Conversation '{request.ConversationId}' not found.");

        return await db.Messages
            .Where(m => m.ConversationId == request.ConversationId && m.TenantId == request.TenantId)
            .OrderBy(m => m.SentAtUtc)
            .Select(m => new ConversationMessageDto(m.Id, m.Direction, m.Sender, m.Body, m.SentAtUtc, m.DeliveryStatus))
            .ToListAsync(cancellationToken);
    }
}
