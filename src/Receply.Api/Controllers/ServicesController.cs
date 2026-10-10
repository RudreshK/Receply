using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Scheduling.Commands.CreateService;
using Receply.Application.Scheduling.Queries.ListServices;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/services")]
public class ServicesController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record CreateServiceRequest(string Name, int DurationMinutes, decimal Price);

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> List(CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return Ok(await sender.Send(new ListServicesQuery(tenantId), cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateServiceRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var id = await sender.Send(new CreateServiceCommand(tenantId, request.Name, request.DurationMinutes, request.Price), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, id);
    }
}
