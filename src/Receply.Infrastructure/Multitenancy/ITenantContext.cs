namespace Receply.Infrastructure.Multitenancy;

/// <summary>Ambient holder for the current request's tenant, set by TenantResolutionMiddleware in the Api layer.</summary>
public interface ITenantContext
{
    Guid? TenantId { get; }
    void SetTenant(Guid tenantId);
}

public class TenantContext : ITenantContext
{
    public Guid? TenantId { get; private set; }

    public void SetTenant(Guid tenantId) => TenantId = tenantId;
}
