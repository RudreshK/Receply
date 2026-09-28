using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Features.Queries.GetTenantFeatures;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

/// <summary>What the dashboard reads once at login to know which subscription-gated UI to show -
/// the reply box, the "Back to AI" toggle, the Translate button.</summary>
[Authorize]
[ApiController]
[Route("api/features")]
public class FeaturesController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<Dictionary<string, bool>>> Get(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var features = await sender.Send(new GetTenantFeaturesQuery(tenantId), cancellationToken);
        return Ok(features.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value));
    }
}
