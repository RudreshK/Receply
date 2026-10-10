using MediatR;

namespace Receply.Application.Staffing.Commands.AddStaffLeave;

public record AddStaffLeaveCommand(
    Guid TenantId,
    Guid? ActingStaffId,
    Guid StaffId,
    DateOnly Date,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Reason) : IRequest<Guid>;
