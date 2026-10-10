using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Platform.Commands.ConvertEnquiry;
using Receply.Application.Platform.Commands.CreateEnquiry;
using Receply.Application.Platform.Commands.DismissEnquiry;
using Receply.Application.Platform.Queries.GetPlatformStats;
using Receply.Application.Platform.Queries.ListBusinesses;
using Receply.Application.Platform.Queries.ListEnquiries;

namespace Receply.Api.Controllers;

/// <summary>The SaaS product owner's cross-tenant console: every business, platform-wide stats, and
/// hand-logged enquiries. The "PlatformAdmin" role is granted to nobody but the product owner - see
/// JwtTokenGenerator.GenerateToken(PlatformAdmin) for why this role never carries a tenant_id claim.</summary>
[Authorize(Roles = "PlatformAdmin")]
[ApiController]
[Route("api/platform-admin")]
public class PlatformAdminController(ISender sender) : ControllerBase
{
    public record CreateEnquiryRequest(string BusinessName, string BusinessType, string ContactName, string PhoneNumber, string? Email, string? Notes);
    public record ConvertEnquiryRequest(string TimeZoneId);

    [HttpGet("businesses")]
    public async Task<ActionResult<List<BusinessListItemDto>>> ListBusinesses(CancellationToken cancellationToken)
        => await sender.Send(new ListBusinessesQuery(), cancellationToken);

    [HttpGet("stats")]
    public async Task<ActionResult<PlatformStatsDto>> GetStats(CancellationToken cancellationToken)
        => await sender.Send(new GetPlatformStatsQuery(), cancellationToken);

    [HttpGet("enquiries")]
    public async Task<ActionResult<List<EnquiryListItemDto>>> ListEnquiries(CancellationToken cancellationToken)
        => await sender.Send(new ListEnquiriesQuery(), cancellationToken);

    [HttpPost("enquiries")]
    public async Task<ActionResult<Guid>> CreateEnquiry(CreateEnquiryRequest request, CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateEnquiryCommand(request.BusinessName, request.BusinessType, request.ContactName, request.PhoneNumber, request.Email, request.Notes),
            cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPost("enquiries/{enquiryId:guid}/convert")]
    public async Task<ActionResult<Guid>> ConvertEnquiry(Guid enquiryId, ConvertEnquiryRequest request, CancellationToken cancellationToken)
    {
        var tenantId = await sender.Send(new ConvertEnquiryCommand(enquiryId, request.TimeZoneId), cancellationToken);
        return Ok(tenantId);
    }

    [HttpPost("enquiries/{enquiryId:guid}/dismiss")]
    public async Task<IActionResult> DismissEnquiry(Guid enquiryId, CancellationToken cancellationToken)
    {
        await sender.Send(new DismissEnquiryCommand(enquiryId), cancellationToken);
        return NoContent();
    }
}
