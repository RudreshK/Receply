using MediatR;

namespace Receply.Application.Staffing.Commands.DeleteStaffLeave;

public record DeleteStaffLeaveCommand(Guid TenantId, Guid? ActingStaffId, Guid LeaveId) : IRequest;
