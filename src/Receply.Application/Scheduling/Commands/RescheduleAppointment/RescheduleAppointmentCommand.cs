using MediatR;

namespace Receply.Application.Scheduling.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(Guid AppointmentId, DateTimeOffset NewStartUtc) : IRequest;
