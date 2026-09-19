using MediatR;

namespace Receply.Application.Scheduling.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid TenantId, Guid AppointmentId, string? Reason) : IRequest;
