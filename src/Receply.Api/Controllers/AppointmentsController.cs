using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Scheduling.Commands.AddAppointmentFollowUp;
using Receply.Application.Scheduling.Commands.BookAppointment;
using Receply.Application.Scheduling.Commands.CancelAppointment;
using Receply.Application.Scheduling.Commands.RescheduleAppointment;
using Receply.Application.Scheduling.Commands.UpdateAppointmentStatus;
using Receply.Application.Scheduling.Queries.GetAppointmentDetail;
using Receply.Application.Scheduling.Queries.ListAppointments;
using Receply.Domain.Scheduling;
using Receply.Infrastructure.Multitenancy;

namespace Receply.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/appointments")]
public class AppointmentsController(ISender sender, ITenantContext tenantContext) : ControllerBase
{
    public record BookAppointmentRequest(Guid ClientId, Guid ServiceId, Guid ResourceId, Guid BranchId, DateTimeOffset StartUtc);
    public record RescheduleAppointmentRequest(DateTimeOffset NewStartUtc);
    public record CancelAppointmentRequest(string? Reason);
    public record UpdateStatusRequest(AppointmentStatus Status);
    public record AddFollowUpRequest(string Instructions, IReadOnlyList<int> DayOffsets);

    [HttpGet]
    public async Task<ActionResult<List<AppointmentListItemDto>>> List(
        [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to,
        [FromQuery] Guid? branchId, [FromQuery] AppointmentStatus? status, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new ListAppointmentsQuery(tenantId, from, to, branchId, status), cancellationToken);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentDetailDto>> GetDetail(Guid id, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        return await sender.Send(new GetAppointmentDetailQuery(tenantId, id), cancellationToken);
    }

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

    /// <summary>Staff recording whether the client actually showed up. Triggers an automatic WhatsApp
    /// message to the client (feature-gated) - a thank-you for Completed, a gentle reschedule ask for NoShow.</summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateStatusRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new UpdateAppointmentStatusCommand(tenantId, id, request.Status), cancellationToken);
        return NoContent();
    }

    /// <summary>Staff/doctor-authored follow-up care instructions, queued to send by WhatsApp 1-3 days
    /// after a completed appointment.</summary>
    [HttpPost("{id:guid}/follow-up")]
    public async Task<IActionResult> AddFollowUp(Guid id, AddFollowUpRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is not { } tenantId)
            return Unauthorized();

        await sender.Send(new AddAppointmentFollowUpCommand(tenantId, id, request.Instructions, request.DayOffsets), cancellationToken);
        return NoContent();
    }
}
