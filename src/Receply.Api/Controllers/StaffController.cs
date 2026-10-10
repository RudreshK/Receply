using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.CreateStaff;
using Receply.Application.Tenancy.Commands.SetStaffActive;
using Receply.Application.Tenancy.Queries.ListStaff;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

/// <summary>Owner-only: who's staff, and the ability to add or deactivate them, is a business-wide
/// decision - not something any staff member logging in should see or change about their colleagues.</summary>
[Authorize(Roles = nameof(StaffRole.Owner))]
[ApiController]
[Route("api/staff")]
public class StaffController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record CreateStaffRequest(string FullName, string? Email, string PhoneNumber, string Role);
    public record SetActiveRequest(bool IsActive);

    [HttpGet]
    public async Task<ActionResult<List<StaffListItemDto>>> List(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListStaffQuery(tenantId), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new CreateStaffCommand(tenantId, request.FullName, request.Email, request.PhoneNumber, request.Role), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPost("{staffId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid staffId, SetActiveRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new SetStaffActiveCommand(tenantId, staffId, request.IsActive), cancellationToken);
        return NoContent();
    }
}
