using MediatR;

namespace Receply.Application.Tenancy.Commands.CreateStaff;

public record CreateStaffCommand(Guid TenantId, string FullName, string? Email, string PhoneNumber, string Role) : IRequest<Guid>;
