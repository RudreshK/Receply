using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Conversations.Commands.ResumeAi;

public class ResumeAiCommandHandler(IApplicationDbContext db, IConversationNotifier conversationNotifier) : IRequestHandler<ResumeAiCommand>
{
    public async Task Handle(ResumeAiCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations.FirstOrDefaultAsync(
            c => c.Id == request.ConversationId && c.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{request.ConversationId}' not found.");

        conversation.ResumeAi();
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.StatusChangedAsync(conversation.TenantId, conversation.Id, conversation.Status, cancellationToken);
    }
}
