using System.Security.Claims;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Middleware;

/// <summary>
/// Resolves the current request's tenant and populates ITenantContext before any Application/EF
/// code runs, so the DbContext's tenant query filter has something to filter by.
///
/// Two sources, in order:
///   1. A "tenant_id" claim on an authenticated JWT (staff dashboard requests).
///   2. An "X-Tenant-Id" header (webhook/service-to-service requests).
///
/// TODO: replace the X-Tenant-Id header with a hashed per-tenant API key lookup once the
/// Tenancy module grows API key management - a raw tenant id header is only safe scaffolding.
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next)
{
    public const string TenantHeaderName = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var tenantId = TryGetTenantFromClaims(context.User) ?? TryGetTenantFromHeader(context.Request);

        if (tenantId.HasValue)
            tenantContext.SetTenant(tenantId.Value);

        await next(context);
    }

    private static Guid? TryGetTenantFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("tenant_id")?.Value;
        return Guid.TryParse(claim, out var tenantId) ? tenantId : null;
    }

    private static Guid? TryGetTenantFromHeader(HttpRequest request)
    {
        var header = request.Headers[TenantHeaderName].FirstOrDefault();
        return Guid.TryParse(header, out var tenantId) ? tenantId : null;
    }
}
