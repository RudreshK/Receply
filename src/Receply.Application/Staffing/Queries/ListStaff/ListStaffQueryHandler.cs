using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Staffing.Queries.ListStaff;

public class ListStaffQueryHandler(IApplicationDbContext db) : IRequestHandler<ListStaffQuery, IReadOnlyList<StaffDto>>
{
    public async Task<IReadOnlyList<StaffDto>> Handle(ListStaffQuery request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff
            .Where(s => s.TenantId == request.TenantId && (request.BranchId == null || s.BranchId == request.BranchId))
            .OrderBy(s => s.FullName)
            .ToListAsync(cancellationToken);

        var staffIds = staff.Select(s => s.Id).ToList();

        var assignments = await (
            from ss in db.StaffServices
            join svc in db.Services on ss.ServiceId equals svc.Id
            where staffIds.Contains(ss.StaffId)
            select new { ss.StaffId, svc.Id, svc.Name, ss.SessionDuration }
        ).ToListAsync(cancellationToken);

        return staff.Select(s => new StaffDto(
            s.Id, s.FullName, s.Email, s.PhoneNumber, s.Role.ToString(), s.BranchId, s.Designation, s.ExperienceYears, s.IsActive,
            assignments.Where(a => a.StaffId == s.Id)
                .Select(a => new StaffServiceDto(a.Id, a.Name, (int)a.SessionDuration.TotalMinutes))
                .ToList()
        )).ToList();
    }
}
