using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Dashboard.Queries.GetDashboardStats;
using Receply.Application.Features;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController(ISender sender, ITenantContext tenantContext, IFeatureAccess featureAccess) : ControllerBase
{
    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        if (!await featureAccess.IsEnabledAsync(tenantId, Feature.PerformanceDashboard, cancellationToken))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "This feature isn't available on your plan." });

        return await sender.Send(new GetDashboardStatsQuery(tenantId), cancellationToken);
    }
}
