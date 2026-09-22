using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.ListBranches;

public class ListBranchesQueryHandler(IApplicationDbContext db) : IRequestHandler<ListBranchesQuery, List<BranchDto>>
{
    public async Task<List<BranchDto>> Handle(ListBranchesQuery request, CancellationToken cancellationToken)
    {
        return await db.Branches
            .Where(b => b.TenantId == request.TenantId)
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto(b.Id, b.Name, b.Address, b.IsActive))
            .ToListAsync(cancellationToken);
    }
}
