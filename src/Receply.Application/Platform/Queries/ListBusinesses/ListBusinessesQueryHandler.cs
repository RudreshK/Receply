using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Platform.Queries.ListBusinesses;

public class ListBusinessesQueryHandler(IApplicationDbContext db) : IRequestHandler<ListBusinessesQuery, List<BusinessListItemDto>>
{
    public async Task<List<BusinessListItemDto>> Handle(ListBusinessesQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var tenants = await db.Tenants.OrderByDescending(t => t.CreatedOn).ToListAsync(cancellationToken);

        return tenants
            .Select(t => new BusinessListItemDto(
                t.Id,
                t.Name,
                t.BusinessType.ToString(),
                t.Status.ToString(),
                t.Plan.ToString(),
                t.HasActiveAccess(now),
                DaysLeft(t, now),
                t.CreatedOn))
            .ToList();
    }

    private static int? DaysLeft(Tenant tenant, DateTimeOffset now)
    {
        var expiresUtc = tenant.Plan == SubscriptionPlan.Trial ? tenant.TrialEndsUtc : tenant.PlanRenewsUtc;
        return expiresUtc is null ? null : (int)Math.Ceiling((expiresUtc.Value - now).TotalDays);
    }
}
