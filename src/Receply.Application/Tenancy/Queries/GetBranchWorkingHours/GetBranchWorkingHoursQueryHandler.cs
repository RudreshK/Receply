using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.GetBranchWorkingHours;

public class GetBranchWorkingHoursQueryHandler(IApplicationDbContext db)
    : IRequestHandler<GetBranchWorkingHoursQuery, List<BranchWorkingHoursDto>>
{
    public async Task<List<BranchWorkingHoursDto>> Handle(GetBranchWorkingHoursQuery request, CancellationToken cancellationToken)
    {
        return await db.BranchWorkingHours
            .Where(h => h.TenantId == request.TenantId && h.BranchId == request.BranchId)
            .OrderBy(h => h.DayOfWeek)
            .Select(h => new BranchWorkingHoursDto(h.DayOfWeek, h.OpenTime, h.CloseTime, h.IsClosed))
            .ToListAsync(cancellationToken);
    }
}
