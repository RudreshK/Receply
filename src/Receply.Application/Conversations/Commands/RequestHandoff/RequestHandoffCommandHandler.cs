using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Conversations.Commands.RequestHandoff;

public class RequestHandoffCommandHandler(IApplicationDbContext db) : IRequestHandler<RequestHandoffCommand>
{
    public async Task Handle(RequestHandoffCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations.FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{request.ConversationId}' not found.");

        conversation.RequestHandoff(request.Reason);

        await db.SaveChangesAsync(cancellationToken);
    }
}
