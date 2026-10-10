using MediatR;

namespace Receply.Application.Staffing.Commands.SetStaffActive;

public record SetStaffActiveCommand(Guid TenantId, Guid? ActingStaffId, Guid StaffId, bool IsActive) : IRequest;
