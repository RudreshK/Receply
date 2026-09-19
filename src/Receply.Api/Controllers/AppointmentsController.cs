using MediatR;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Scheduling.Commands.BookAppointment;
using Receply.Application.Scheduling.Commands.CancelAppointment;
using Receply.Application.Scheduling.Commands.RescheduleAppointment;

namespace Receply.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController(ISender sender) : ControllerBase
{
    public record BookAppointmentRequest(Guid TenantId, Guid CustomerId, Guid ServiceId, Guid ResourceId, Guid LocationId, DateTimeOffset StartUtc);
    public record RescheduleAppointmentRequest(DateTimeOffset NewStartUtc);
    public record CancelAppointmentRequest(string? Reason);

    [HttpPost]
    public async Task<ActionResult<Guid>> Book(BookAppointmentRequest request, CancellationToken cancellationToken)
    {
        var command = new BookAppointmentCommand(
            request.TenantId, request.CustomerId, request.ServiceId, request.ResourceId, request.LocationId, request.StartUtc);

        var appointmentId = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Book), new { id = appointmentId }, appointmentId);
    }

    [HttpPatch("{id:guid}/reschedule")]
    public async Task<IActionResult> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken cancellationToken)
    {
        await sender.Send(new RescheduleAppointmentCommand(id, request.NewStartUtc), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest? request, CancellationToken cancellationToken)
    {
        await sender.Send(new CancelAppointmentCommand(id, request?.Reason), cancellationToken);
        return NoContent();
    }
}
