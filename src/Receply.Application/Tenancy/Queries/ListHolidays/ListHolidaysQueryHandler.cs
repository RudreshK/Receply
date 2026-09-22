using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.ListHolidays;

public class ListHolidaysQueryHandler(IApplicationDbContext db) : IRequestHandler<ListHolidaysQuery, List<HolidayDto>>
{
    public async Task<List<HolidayDto>> Handle(ListHolidaysQuery request, CancellationToken cancellationToken)
    {
        return await db.Holidays
            .Where(h => h.TenantId == request.TenantId && h.Date >= request.From && h.Date <= request.To)
            .OrderBy(h => h.Date)
            .Select(h => new HolidayDto(h.Id, h.BranchId, h.Date, h.Name))
            .ToListAsync(cancellationToken);
    }
}
