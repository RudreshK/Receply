using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.CreateBranch;
using Receply.Application.Tenancy.Commands.SetBranchWorkingHours;
using Receply.Application.Tenancy.Queries.GetBranchWorkingHours;
using Receply.Application.Tenancy.Queries.ListBranches;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/branches")]
public class BranchesController(ISender sender, ITenantContext tenantContext, ICurrentUserContext currentUserContext) : ControllerBase
{
    public record SetWorkingHoursRequest(List<BranchWorkingHoursEntry> Entries);
    public record CreateBranchRequest(string Name, string? Address);

    [HttpGet]
    public async Task<ActionResult<List<BranchDto>>> List(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListBranchesQuery(tenantId), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateBranchRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new CreateBranchCommand(tenantId, currentUserContext.StaffId, request.Name, request.Address ?? ""), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }

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
