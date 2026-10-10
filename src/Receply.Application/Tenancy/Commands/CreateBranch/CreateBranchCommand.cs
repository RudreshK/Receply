using MediatR;

namespace Receply.Application.Tenancy.Commands.CreateBranch;

public record CreateBranchCommand(Guid TenantId, Guid? ActingStaffId, string Name, string Address) : IRequest<Guid>;
