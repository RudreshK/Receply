using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.ChangeSubscriptionPlan;
using Receply.Application.Tenancy.Commands.UpdateBranding;
using Receply.Application.Tenancy.Commands.UpdateTenantSettings;
using Receply.Application.Tenancy.Queries.GetBranding;
using Receply.Application.Tenancy.Queries.GetSubscriptionStatus;
using Receply.Application.Tenancy.Queries.GetTenantSettings;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/tenant")]
public class TenantsController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record UpdateTenantSettingsRequest(string Name, string BusinessType, string TimeZoneId);
    public record ChangePlanRequest(string Plan);
    public record UpdateBrandingRequest(
        string Name,
        string? BusinessEmail,
        string? Website,
        string? LogoDataUrl,
        string? OwnerName,
        string? Designation,
        string? OwnerPhotoDataUrl);

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

    [HttpGet("branding")]
    public async Task<ActionResult<BrandingDto>> GetBranding(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetBrandingQuery(tenantId), cancellationToken);
    }

    /// <summary>Owner-only: branding (name/logo/etc. shown in place of "Receply") is a business-wide
    /// identity decision, not something any staff member logging in should be able to change.</summary>
    [Authorize(Roles = nameof(StaffRole.Owner))]
    [HttpPut("branding")]
    public async Task<IActionResult> UpdateBranding(UpdateBrandingRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(
            new UpdateBrandingCommand(
                tenantId,
                request.Name,
                request.BusinessEmail,
                request.Website,
                request.LogoDataUrl,
                request.OwnerName,
                request.Designation,
                request.OwnerPhotoDataUrl),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<SubscriptionStatusDto>> GetSubscription(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetSubscriptionStatusQuery(tenantId), cancellationToken);
    }

    /// <summary>Owner-only: no payment gateway exists yet, so this is a self-serve/honor-system plan
    /// switch - billing today is settled offline.</summary>
    [Authorize(Roles = nameof(StaffRole.Owner))]
    [HttpPut("subscription")]
    public async Task<IActionResult> ChangePlan(ChangePlanRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new ChangeSubscriptionPlanCommand(tenantId, request.Plan), cancellationToken);
        return NoContent();
    }
}
