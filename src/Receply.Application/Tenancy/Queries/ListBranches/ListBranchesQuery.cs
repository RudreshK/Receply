using MediatR;

namespace Receply.Application.Tenancy.Queries.ListBranches;

public record BranchDto(Guid Id, string Name, string Address, bool IsActive);

public record ListBranchesQuery(Guid TenantId) : IRequest<List<BranchDto>>;
