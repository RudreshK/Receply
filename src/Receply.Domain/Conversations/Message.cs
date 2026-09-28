using Receply.Domain.Common;

namespace Receply.Domain.Conversations;

public enum MessageDirection
{
    Inbound,
    Outbound
}

public enum MessageSender
{
    Client,
    Ai,
    Human
}

/// <summary>
/// Delivery state of an outbound message as reported by the channel provider's status webhooks
/// (WhatsApp: sent/delivered/read). Meaningless for inbound messages, which stay Pending.
/// Ranked in the order a message normally progresses through, used to ignore late/out-of-order
/// webhook deliveries that would otherwise regress an already-newer status.
/// </summary>
public enum MessageDeliveryStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed
}

public class Message : TenantOwnedEntity
{
    public Guid ConversationId { get; private set; }
    public MessageDirection Direction { get; private set; }
    public MessageSender Sender { get; private set; }
    public string Body { get; private set; } = default!;
    public DateTimeOffset SentAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    /// <summary>The provider's message id (WhatsApp's "wamid") - set once we know it, used to match
    /// later status webhooks back to this message.</summary>
    public string? ExternalMessageId { get; private set; }
    public MessageDeliveryStatus DeliveryStatus { get; private set; } = MessageDeliveryStatus.Pending;

    private Message() { }

    internal static Message Create(Guid tenantId, Guid conversationId, MessageDirection direction, MessageSender sender, string body, string? externalMessageId = null)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Message body is required.", nameof(body));

        var message = new Message
        {
            ConversationId = conversationId,
            Direction = direction,
            Sender = sender,
            Body = body,
            ExternalMessageId = externalMessageId
        };
        message.TenantId = tenantId;
        return message;
    }

    /// <summary>Records the provider's message id once we've actually handed the message off to send.</summary>
    public void MarkSent(string externalMessageId)
    {
        ExternalMessageId = externalMessageId;
        UpdateDeliveryStatus(MessageDeliveryStatus.Sent);
    }

    /// <summary>Applies a status reported by the provider's webhook, ignoring anything that isn't
    /// further along than what we already know (statuses can arrive out of order).</summary>
    public void UpdateDeliveryStatus(MessageDeliveryStatus status)
    {
        if (Rank(status) <= Rank(DeliveryStatus))
            return;

        DeliveryStatus = status;
    }

    private static int Rank(MessageDeliveryStatus status) => status switch
    {
        MessageDeliveryStatus.Pending => 0,
        MessageDeliveryStatus.Sent => 1,
        MessageDeliveryStatus.Delivered => 2,
        MessageDeliveryStatus.Read => 3,
        MessageDeliveryStatus.Failed => 3,
        _ => 0
    };
}
