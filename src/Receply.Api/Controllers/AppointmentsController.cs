using MediatR;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Scheduling.Commands.BookAppointment;
using Receply.Application.Scheduling.Commands.CancelAppointment;
using Receply.Application.Scheduling.Commands.RescheduleAppointment;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record BookAppointmentRequest(Guid ClientId, Guid ServiceId, Guid ResourceId, Guid BranchId, DateTimeOffset StartUtc);
    public record RescheduleAppointmentRequest(DateTimeOffset NewStartUtc);
    public record CancelAppointmentRequest(string? Reason);

    [HttpPost]
    public async Task<ActionResult<Guid>> Book(BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        var command = new BookAppointmentCommand(
            tenantId, request.ClientId, request.ServiceId, request.ResourceId, request.BranchId, request.StartUtc);

        var appointmentId = await sender.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, appointmentId);
    }

    [HttpPatch("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new RescheduleAppointmentCommand(tenantId, id, request.NewStartUtc), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest? request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new CancelAppointmentCommand(tenantId, id, request?.Reason), cancellationToken);
        return NoContent();
    }
}
