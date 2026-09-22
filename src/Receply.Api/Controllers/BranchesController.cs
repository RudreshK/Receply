using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.SetBranchWorkingHours;
using Receply.Application.Tenancy.Queries.GetBranchWorkingHours;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/branches")]
public class BranchesController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record SetWorkingHoursRequest(List<BranchWorkingHoursEntry> Entries);

    [HttpGet("{branchId:guid}/working-hours")]
    public async Task<ActionResult<List<BranchWorkingHoursDto>>> GetWorkingHours(Guid branchId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetBranchWorkingHoursQuery(tenantId, branchId), cancellationToken);
    }

    [HttpPut("{branchId:guid}/working-hours")]
    public async Task<IActionResult> SetWorkingHours(Guid branchId, SetWorkingHoursRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new SetBranchWorkingHoursCommand(tenantId, branchId, request.Entries), cancellationToken);
        return NoContent();
    }
}
