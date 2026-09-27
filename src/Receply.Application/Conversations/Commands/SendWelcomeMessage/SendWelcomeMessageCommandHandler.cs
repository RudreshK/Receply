using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Commands.SendWelcomeMessage;

public class SendWelcomeMessageCommandHandler(IApplicationDbContext db, IChannelProvider channelProvider)
    : IRequestHandler<SendWelcomeMessageCommand>
{
    public async Task Handle(SendWelcomeMessageCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId && c.TenantId == request.TenantId, cancellationToken);

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken);
        var client = conversation is null ? null
            : await db.Clients.FirstOrDefaultAsync(c => c.Id == conversation.ClientId, cancellationToken);
        var channelAccount = conversation is null ? null
            : await db.ChannelAccounts.FirstOrDefaultAsync(c => c.Id == conversation.ChannelAccountId, cancellationToken);

        if (conversation is null || tenant is null || client is null || channelAccount is null)
            return;

        var welcomeText = $"Hi! Welcome to {tenant.Name} 👋 I'm your virtual receptionist, here 24/7 to help you " +
            "book, reschedule, or cancel appointments, and answer questions about our services. How can I help you today?";

        // Explicit Add matters: appending to an already-tracked (queried) parent's collection is
        // ambiguous for a client-generated GUID key - EF Core can mark the new child Modified
        // instead of Added, then issue a doomed UPDATE against a row that doesn't exist yet.
        var message = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Ai, welcomeText);
        db.Messages.Add(message);
        await db.SaveChangesAsync(cancellationToken);

        await channelProvider.SendMessageAsync(channelAccount, client.PhoneNumber, welcomeText, cancellationToken);
    }
}
