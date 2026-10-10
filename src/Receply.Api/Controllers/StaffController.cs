using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Staffing.Commands.AddStaff;
using Receply.Application.Staffing.Commands.AddStaffLeave;
using Receply.Application.Staffing.Commands.DeleteStaffLeave;
using Receply.Application.Staffing.Commands.SetStaffActive;
using Receply.Application.Staffing.Commands.UpdateStaff;
using Receply.Application.Staffing.Queries.ListStaff;
using Receply.Application.Staffing.Queries.ListStaffLeaves;
using Receply.Domain.Tenancy;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/staff")]
public class StaffController(ISender sender, ITenantContext tenantContext, ICurrentUserContext currentUserContext) : ControllerBase
{
    public record AddStaffRequest(
        string FullName, string Email, string PhoneNumber, StaffRole Role, Guid? BranchId,
        string? Designation, int ExperienceYears, List<StaffServiceInput> Services);

    public record UpdateStaffRequest(string FullName, string Email, string? Designation, int ExperienceYears, List<StaffServiceInput> Services);

    public record SetActiveRequest(bool IsActive);

    public record AddLeaveRequest(DateOnly Date, TimeOnly? StartTime, TimeOnly? EndTime, string? Reason);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffDto>>> List([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return Ok(await sender.Send(new ListStaffQuery(tenantId, branchId), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Add(AddStaffRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new AddStaffCommand(
            tenantId, currentUserContext.StaffId, request.FullName, request.Email, request.PhoneNumber,
            request.Role, request.BranchId, request.Designation, request.ExperienceYears, request.Services), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpPut("{staffId:guid}")]
    public async Task<IActionResult> Update(Guid staffId, UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new UpdateStaffCommand(
            tenantId, currentUserContext.StaffId, staffId, request.FullName, request.Email,
            request.Designation, request.ExperienceYears, request.Services), cancellationToken);

        return NoContent();
    }

    [HttpPost("{staffId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid staffId, SetActiveRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new SetStaffActiveCommand(tenantId, currentUserContext.StaffId, staffId, request.IsActive), cancellationToken);
        return NoContent();
    }

    [HttpGet("{staffId:guid}/leaves")]
    public async Task<ActionResult<IReadOnlyList<StaffLeaveDto>>> ListLeaves(
        Guid staffId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return Ok(await sender.Send(new ListStaffLeavesQuery(tenantId, staffId, from, to), cancellationToken));
    }

    [HttpPost("{staffId:guid}/leaves")]
    public async Task<ActionResult<Guid>> AddLeave(Guid staffId, AddLeaveRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new AddStaffLeaveCommand(
            tenantId, currentUserContext.StaffId, staffId, request.Date, request.StartTime, request.EndTime, request.Reason), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpDelete("leaves/{leaveId:guid}")]
    public async Task<IActionResult> DeleteLeave(Guid leaveId, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new DeleteStaffLeaveCommand(tenantId, currentUserContext.StaffId, leaveId), cancellationToken);
        return NoContent();
    }
}
