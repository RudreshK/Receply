using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.SetStaffActive;

public class SetStaffActiveCommandHandler(IApplicationDbContext db) : IRequestHandler<SetStaffActiveCommand>
{
    public async Task Handle(SetStaffActiveCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        if (staff.Role == StaffRole.Owner)
            throw new InvalidOperationException("The business owner's account can't be deactivated.");

        if (request.IsActive) staff.Activate();
        else staff.Deactivate();

        await db.SaveChangesAsync(cancellationToken);
    }
}
