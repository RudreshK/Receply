using MediatR;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Commands.UpdateAppointmentStatus;

/// <summary>Staff marking an appointment's real-world outcome after it's over - the client showed up
/// (Completed) or didn't (NoShow). Kept generic on purpose: it reads the same for a clinic, a salon,
/// or a consultant, since it's just "did the appointment happen".</summary>
public record UpdateAppointmentStatusCommand(Guid TenantId, Guid AppointmentId, AppointmentStatus NewStatus) : IRequest;
