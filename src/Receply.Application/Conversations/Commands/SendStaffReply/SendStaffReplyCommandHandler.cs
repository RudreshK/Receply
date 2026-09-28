using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Application.Conversations.Queries.GetConversationMessages;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations.Commands.SendStaffReply;

public class SendStaffReplyCommandHandler(
    IApplicationDbContext db,
    IChannelProvider channelProvider,
    IConversationNotifier conversationNotifier) : IRequestHandler<SendStaffReplyCommand>
{
    public async Task Handle(SendStaffReplyCommand request, CancellationToken cancellationToken)
    {
        var conversation = await db.Conversations.FirstOrDefaultAsync(
            c => c.Id == request.ConversationId && c.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Conversation '{request.ConversationId}' not found.");

        var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == conversation.ClientId, cancellationToken)
            ?? throw new KeyNotFoundException($"Client for conversation '{request.ConversationId}' not found.");

        var channelAccount = await db.ChannelAccounts.FirstOrDefaultAsync(c => c.Id == conversation.ChannelAccountId, cancellationToken)
            ?? throw new KeyNotFoundException($"Channel account for conversation '{request.ConversationId}' not found.");

        // Replying by hand takes the thread over from the AI, whatever state it was in - the AI stays
        // quiet on this conversation until a staff member explicitly hands it back (ResumeAiCommand).
        if (conversation.Status is not HandoffStatus.HumanAssigned)
            conversation.AssignHuman(request.StaffId);

        var message = conversation.AddMessage(MessageDirection.Outbound, MessageSender.Human, request.Body);
        await db.SaveChangesAsync(cancellationToken);

        await conversationNotifier.MessageAddedAsync(
            conversation.TenantId,
            conversation.Id,
            new ConversationMessageDto(message.Id, message.Direction, message.Sender, message.Body, message.SentAtUtc, message.DeliveryStatus),
            cancellationToken);

        await conversationNotifier.StatusChangedAsync(conversation.TenantId, conversation.Id, conversation.Status, cancellationToken);

        await ConversationMessagingHelper.SendAndTrackAsync(
            db, channelProvider, conversationNotifier, conversation, message, channelAccount, client.PhoneNumber, request.Body, cancellationToken);
    }
}
