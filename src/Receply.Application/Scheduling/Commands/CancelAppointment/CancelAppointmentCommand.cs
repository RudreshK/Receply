using MediatR;

namespace Receply.Application.Scheduling.Commands.CancelAppointment;

public record CancelAppointmentCommand(Guid AppointmentId, string? Reason) : IRequest;
