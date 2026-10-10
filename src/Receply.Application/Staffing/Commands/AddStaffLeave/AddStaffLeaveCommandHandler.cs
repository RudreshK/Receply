using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Staffing.Commands.AddStaffLeave;

public class AddStaffLeaveCommandHandler(IApplicationDbContext db) : IRequestHandler<AddStaffLeaveCommand, Guid>
{
    public async Task<Guid> Handle(AddStaffLeaveCommand request, CancellationToken cancellationToken)
    {
        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        var target = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.StaffId && s.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Staff '{request.StaffId}' not found.");

        StaffAccessGuard.EnsureCanManage(caller, target.BranchId);

        var alreadyOnLeave = await db.StaffLeaves.AnyAsync(l => l.StaffId == target.Id && l.Date == request.Date, cancellationToken);
        if (alreadyOnLeave)
            throw new InvalidOperationException("This staff member already has a leave recorded for that date.");

        var leave = StaffLeave.Create(request.TenantId, target.Id, request.Date, request.StartTime, request.EndTime, request.Reason);
        db.StaffLeaves.Add(leave);
        await db.SaveChangesAsync(cancellationToken);

        return leave.Id;
    }
}
