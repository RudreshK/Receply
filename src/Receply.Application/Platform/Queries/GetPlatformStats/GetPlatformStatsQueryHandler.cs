using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Platform;

namespace Receply.Application.Platform.Queries.GetPlatformStats;

public class GetPlatformStatsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetPlatformStatsQuery, PlatformStatsDto>
{
    public async Task<PlatformStatsDto> Handle(GetPlatformStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startOfThisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var startOfThisWeek = now.AddDays(-7);

        var total = await db.Tenants.CountAsync(cancellationToken);
        var newThisWeek = await db.Tenants.CountAsync(t => t.CreatedOn >= startOfThisWeek, cancellationToken);
        var newThisMonth = await db.Tenants.CountAsync(t => t.CreatedOn >= startOfThisMonth, cancellationToken);
        var pendingEnquiries = await db.BusinessEnquiries.CountAsync(e => e.Status == EnquiryStatus.New, cancellationToken);

        return new PlatformStatsDto(total, newThisWeek, newThisMonth, pendingEnquiries);
    }
}
