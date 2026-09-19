using Receply.Domain.Common;

namespace Receply.Domain.Conversations;

public enum HandoffStatus
{
    AiHandled,
    HandoffRequested,
    HumanAssigned,
    Resolved
}

public class Conversation : TenantOwnedEntity
{
    public Guid CustomerId { get; private set; }
    public Guid ChannelAccountId { get; private set; }
    public HandoffStatus Status { get; private set; } = HandoffStatus.AiHandled;
    public Guid? AssignedStaffMemberId { get; private set; }
    public DateTimeOffset StartedAtUtc { get; private set; } = DateTimeOffset.UtcNow;

    private readonly List<Message> _messages = [];
    public IReadOnlyCollection<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation Start(Guid tenantId, Guid customerId, Guid channelAccountId)
    {
        var conversation = new Conversation
        {
            CustomerId = customerId,
            ChannelAccountId = channelAccountId
        };
        conversation.TenantId = tenantId;
        return conversation;
    }

    public Message AddMessage(MessageDirection direction, MessageSender sender, string body)
    {
        var message = Message.Create(TenantId, Id, direction, sender, body);
        _messages.Add(message);
        return message;
    }

    public void RequestHandoff(string reason)
    {
        if (Status is HandoffStatus.HumanAssigned)
            return; // already with a human, nothing to do

        Status = HandoffStatus.HandoffRequested;
        Raise(new HandoffRequested(TenantId, Id, reason));
    }

    public void AssignHuman(Guid staffMemberId)
    {
        Status = HandoffStatus.HumanAssigned;
        AssignedStaffMemberId = staffMemberId;
        Raise(new HandoffAssigned(TenantId, Id, staffMemberId));
    }

    public void Resolve()
    {
        Status = HandoffStatus.Resolved;
        Raise(new ConversationResolved(TenantId, Id));
    }
}
