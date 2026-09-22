using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.UpdateTenantSettings;
using Receply.Application.Tenancy.Queries.GetTenantSettings;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class TenantsController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record UpdateTenantSettingsRequest(string Name, string BusinessType, string TimeZoneId);

    [HttpGet("settings")]
    public async Task<ActionResult<TenantSettingsDto>> GetSettings(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetTenantSettingsQuery(tenantId), cancellationToken);
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings(UpdateTenantSettingsRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new UpdateTenantSettingsCommand(tenantId, request.Name, request.BusinessType, request.TimeZoneId), cancellationToken);
        return NoContent();
    }
}
