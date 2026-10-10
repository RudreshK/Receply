using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Staffing.Commands.SetStaffActive;

public class SetStaffActiveCommandHandler(IApplicationDbContext db) : IRequestHandler<SetStaffActiveCommand>
{
    public async Task Handle(SetStaffActiveCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        var target = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        StaffAccessGuard.EnsureCanManage(caller, target.BranchId);

        if (request.IsActive)
            target.Activate();
        else
            target.Deactivate();

        // A deactivated staff member's bookable calendar goes with them - no new slots should be
        // offered against it, same as a leave day but indefinite instead of one date.
        var resource = await db.Resources.FirstOrDefaultAsync(r => r.StaffId == target.Id, cancellationToken);
        if (resource is not null)
        {
            if (request.IsActive)
                resource.Activate();
            else
                resource.Deactivate();
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
