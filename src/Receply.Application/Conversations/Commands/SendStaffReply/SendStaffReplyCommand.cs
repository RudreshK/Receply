using MediatR;

namespace Receply.Application.Conversations.Commands.SendStaffReply;

/// <summary>A staff member replying to a customer by hand from the dashboard, while the AI stays quiet.</summary>
public record SendStaffReplyCommand(Guid TenantId, Guid StaffId, Guid ConversationId, string Body) : IRequest;
