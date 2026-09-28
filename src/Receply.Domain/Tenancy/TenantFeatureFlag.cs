using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>
/// One tenant's on/off switch for one Feature. Absence of a row for a (tenant, feature) pair means
/// "use the feature's default" (see IFeatureAccess) - a row only needs to exist once a tenant's
/// subscription plan deliberately turns something on or off from that default.
/// </summary>
public class TenantFeatureFlag : TenantOwnedEntity
{
    public Feature Feature { get; private set; }
    public bool IsEnabled { get; private set; }

    private TenantFeatureFlag() { }

    public static TenantFeatureFlag Create(Guid tenantId, Feature feature, bool isEnabled)
    {
        var flag = new TenantFeatureFlag { Feature = feature, IsEnabled = isEnabled };
        flag.TenantId = tenantId;
        return flag;
    }

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;
}
