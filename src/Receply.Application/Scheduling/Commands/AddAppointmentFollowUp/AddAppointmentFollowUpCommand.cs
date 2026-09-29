using MediatR;

namespace Receply.Application.Scheduling.Commands.AddAppointmentFollowUp;

/// <summary>Staff-authored care instructions (medication, diet, exercise, ...) queued to reach the
/// client by WhatsApp on the given days after a completed appointment - 1 to 3 days out.</summary>
public record AddAppointmentFollowUpCommand(
    Guid TenantId, Guid AppointmentId, string Instructions, IReadOnlyList<int> DayOffsets) : IRequest;
