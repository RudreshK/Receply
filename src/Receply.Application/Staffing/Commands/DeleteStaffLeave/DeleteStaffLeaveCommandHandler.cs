using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Staffing.Commands.DeleteStaffLeave;

public class DeleteStaffLeaveCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteStaffLeaveCommand>
{
    public async Task Handle(DeleteStaffLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await db.StaffLeaves.FirstOrDefaultAsync(l => l.Id == request.LeaveId && l.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave '{request.LeaveId}' not found.");

        var caller = await db.Staff.FirstOrDefaultAsync(s => s.Id == request.ActingStaffId && s.TenantId == request.TenantId, cancellationToken);
        var target = await db.Staff.FirstOrDefaultAsync(s => s.Id == leave.StaffId && s.TenantId == request.TenantId, cancellationToken);

        StaffAccessGuard.EnsureCanManage(caller, target?.BranchId);

        db.StaffLeaves.Remove(leave);
        await db.SaveChangesAsync(cancellationToken);
    }
}
