using Receply.Domain.Common;

namespace Receply.Domain.Conversations;

public enum MessageDirection
{
    Inbound,
    Outbound
}

public enum MessageSender
{
    Customer,
    Ai,
    Human
}

public class Message : TenantOwnedEntity
{
    public Guid ConversationId { get; private set; }
    public MessageDirection Direction { get; private set; }
    public MessageSender Sender { get; private set; }
    public string Body { get; private set; } = default!;
    public DateTimeOffset SentAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private Message() { }

    internal static Message Create(Guid tenantId, Guid conversationId, MessageDirection direction, MessageSender sender, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Message body is required.", nameof(body));

        var message = new Message
        {
            ConversationId = conversationId,
            Direction = direction,
            Sender = sender,
            Body = body
        };
        message.TenantId = tenantId;
        return message;
    }
}
