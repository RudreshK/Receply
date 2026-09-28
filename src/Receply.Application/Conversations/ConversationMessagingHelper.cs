using Receply.Application.Channels;
using Receply.Application.Common;
using Receply.Domain.Channels;
using Receply.Domain.Conversations;

namespace Receply.Application.Conversations;

/// <summary>
/// Shared "actually send it, then record what happened" tail used by every place that hands an
/// already-saved outbound Message off to the channel provider (the AI reply pipeline, a staff
/// member's manual reply). Keeps the send/track/notify sequence - and its failure handling - in one
/// place instead of duplicated per caller.
/// </summary>
public static class ConversationMessagingHelper
{
    public static async Task SendAndTrackAsync(
        IApplicationDbContext db,
        IChannelProvider channelProvider,
        IConversationNotifier conversationNotifier,
        Conversation conversation,
        Message message,
        ChannelAccount channelAccount,
        string toPhoneNumber,
        string body,
        CancellationToken cancellationToken)
    {
        try
        {
            var externalMessageId = await channelProvider.SendMessageAsync(channelAccount, toPhoneNumber, body, cancellationToken);
            message.MarkSent(externalMessageId);
        }
        catch
        {
            message.UpdateDeliveryStatus(MessageDeliveryStatus.Failed);
            await db.SaveChangesAsync(cancellationToken);
            await conversationNotifier.MessageStatusChangedAsync(conversation.TenantId, conversation.Id, message.Id, message.DeliveryStatus, cancellationToken);
            throw;
        }

        await db.SaveChangesAsync(cancellationToken);
        await conversationNotifier.MessageStatusChangedAsync(conversation.TenantId, conversation.Id, message.Id, message.DeliveryStatus, cancellationToken);
    }
}
