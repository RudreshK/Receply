using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Dashboard.Queries.GetDashboardStats;

public class GetDashboardStatsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetDashboardStatsQuery, DashboardStatsDto>
{
    public async Task<DashboardStatsDto> Handle(GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfLastMonth = startOfThisMonth.AddMonths(-1);

        var clients = db.Clients.Where(c => c.TenantId == request.TenantId);

        var totalClients = await clients.CountAsync(cancellationToken);
        var newThisMonth = await clients.CountAsync(c => c.CreatedOn >= startOfThisMonth, cancellationToken);
        var newLastMonth = await clients.CountAsync(c => c.CreatedOn >= startOfLastMonth && c.CreatedOn < startOfThisMonth, cancellationToken);

        return new DashboardStatsDto(totalClients, newThisMonth, newLastMonth);
    }
}
