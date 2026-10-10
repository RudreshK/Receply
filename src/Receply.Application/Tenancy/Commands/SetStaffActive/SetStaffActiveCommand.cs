using MediatR;

namespace Receply.Application.Tenancy.Commands.SetStaffActive;

public record SetStaffActiveCommand(Guid TenantId, Guid StaffId, bool IsActive) : IRequest;
