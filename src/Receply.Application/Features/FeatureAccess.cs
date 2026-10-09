using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features;

public class FeatureAccess(IApplicationDbContext db) : IFeatureAccess
{
    public async Task<bool> IsEnabledAsync(Guid tenantId, Feature feature, CancellationToken cancellationToken = default)
    {
        // An explicit per-tenant override (a support/back-office comp, or a deliberate restriction)
        // always wins over whatever the tenant's plan would otherwise say.
        var flag = await db.TenantFeatureFlags
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Feature == feature, cancellationToken);

        if (flag is not null)
            return flag.IsEnabled;

        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);
        return tenant is not null && SubscriptionPlans.Includes(tenant.Plan, feature);
    }
}
