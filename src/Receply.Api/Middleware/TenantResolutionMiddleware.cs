using System.Security.Claims;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Middleware;

/// <summary>
/// Resolves the current request's tenant and (when available) the staff member driving it,
/// before any Application/EF code runs - so the DbContext's tenant query filter and the audit
/// interceptor's CreatedBy/ModifiedBy stamping both have something to work with.
///
/// Tenant, two sources in order:
///   1. A "tenant_id" claim on an authenticated JWT (staff dashboard requests).
///   2. An "X-Tenant-Id" header (webhook/service-to-service requests).
///
/// Staff actor: a "staff_id" claim on an authenticated JWT. Left unset for webhook/background-queue
/// paths - correctly represents "system/AI-originated" for audit purposes.
///
/// TODO: replace the X-Tenant-Id header with a hashed per-tenant API key lookup once the
/// Tenancy module grows API key management - a raw tenant id header is only safe scaffolding.
/// </summary>
public class TenantResolutionMiddleware(RequestDelegate next)
{
    public const string TenantHeaderName = "X-Tenant-Id";

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ICurrentUserContext currentUserContext)
    {
        var tenantId = TryGetTenantFromClaims(context.User) ?? TryGetTenantFromHeader(context.Request);

        if (tenantId.HasValue)
            tenantContext.SetTenant(tenantId.Value);

        if (TryGetStaffFromClaims(context.User) is { } staffId)
            currentUserContext.SetStaff(staffId);

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

    private static Guid? TryGetStaffFromClaims(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("staff_id")?.Value;
        return Guid.TryParse(claim, out var staffId) ? staffId : null;
    }
}
