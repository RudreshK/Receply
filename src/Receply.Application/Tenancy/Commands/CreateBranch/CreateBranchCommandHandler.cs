using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.CreateBranch;

public class CreateBranchCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateBranchCommand, Guid>
{
    public async Task<Guid> Handle(CreateBranchCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        if (caller is null || caller.Role != StaffRole.Owner)
            throw new UnauthorizedAccessException("Only the business owner can add a branch.");

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        var branch = tenant.AddBranch(request.Name, request.Address);

        // Tenant.Branches isn't mapped as an EF navigation (see BranchConfiguration - Branch only
        // carries a plain TenantId, no HasMany back on Tenant), so adding to the in-memory
        // collection alone wouldn't be tracked - it must be added to its own DbSet explicitly.
        db.Branches.Add(branch);
        await db.SaveChangesAsync(cancellationToken);

        return branch.Id;
    }
}
