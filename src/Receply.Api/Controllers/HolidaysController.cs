using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Tenancy.Commands.CreateHoliday;
using Receply.Application.Tenancy.Commands.DeleteHoliday;
using Receply.Application.Tenancy.Queries.ListHolidays;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/holidays")]
public class HolidaysController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record CreateHolidayRequest(Guid? BranchId, DateOnly Date, string Name);

    [HttpGet]
    public async Task<ActionResult<List<HolidayDto>>> List([FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListHolidaysQuery(tenantId, from, to), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateHolidayRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new CreateHolidayCommand(tenantId, request.BranchId, request.Date, request.Name), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new DeleteHolidayCommand(tenantId, id), cancellationToken);
        return NoContent();
    }
}
