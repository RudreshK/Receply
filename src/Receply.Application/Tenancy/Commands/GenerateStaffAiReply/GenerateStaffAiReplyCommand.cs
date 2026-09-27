using MediatR;

namespace Receply.Application.Tenancy.Commands.GenerateStaffAiReply;

/// <summary>
/// Handles one staff-originated WhatsApp message (holidays, own-details updates) and sends the reply
/// itself - unlike the customer flow, staff commands are stateless single-turn requests, not part of
/// a persisted Conversation.
/// </summary>
public record GenerateStaffAiReplyCommand(Guid TenantId, Guid StaffId, string MessageBody) : IRequest;
