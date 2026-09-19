using MediatR;

namespace Receply.Application.Scheduling.Commands.RescheduleAppointment;

public record RescheduleAppointmentCommand(Guid TenantId, Guid AppointmentId, DateTimeOffset NewStartUtc) : IRequest;
