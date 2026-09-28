using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features;

public class FeatureAccess(IApplicationDbContext db) : IFeatureAccess
{
    public async Task<bool> IsEnabledAsync(Guid tenantId, Feature feature, CancellationToken cancellationToken = default)
    {
        var flag = await db.TenantFeatureFlags
            .FirstOrDefaultAsync(f => f.TenantId == tenantId && f.Feature == feature, cancellationToken);

        return flag?.IsEnabled ?? DefaultEnabled(feature);
    }

    /// <summary>What a feature is worth to a tenant before their plan has ever said otherwise. Every
    /// feature here defaults on today because there's no billing/plan system driving these flags
    /// yet - once one exists, it writes a TenantFeatureFlag row per tenant per plan instead of
    /// relying on this fallback.</summary>
    private static bool DefaultEnabled(Feature feature) => true;
}
