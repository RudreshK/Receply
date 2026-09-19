using MediatR;

namespace Receply.Domain.Conversations;

public record HandoffRequested(Guid TenantId, Guid ConversationId, string Reason) : INotification;

public record HandoffAssigned(Guid TenantId, Guid ConversationId, Guid StaffMemberId) : INotification;

public record ConversationResolved(Guid TenantId, Guid ConversationId) : INotification;
