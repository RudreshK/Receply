using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Staffing.Queries.ListStaffLeaves;

public class ListStaffLeavesQueryHandler(IApplicationDbContext db) : IRequestHandler<ListStaffLeavesQuery, IReadOnlyList<StaffLeaveDto>>
{
    public async Task<IReadOnlyList<StaffLeaveDto>> Handle(ListStaffLeavesQuery request, CancellationToken cancellationToken)
    {
        return await db.StaffLeaves
            .Where(l => l.TenantId == request.TenantId && l.StaffId == request.StaffId && l.Date >= request.From && l.Date <= request.To)
            .OrderBy(l => l.Date)
            .Select(l => new StaffLeaveDto(l.Id, l.StaffId, l.Date, l.StartTime, l.EndTime, l.Reason))
            .ToListAsync(cancellationToken);
    }
}
