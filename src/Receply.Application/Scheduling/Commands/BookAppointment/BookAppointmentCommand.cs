using MediatR;

namespace Receply.Application.Scheduling.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid TenantId,
    Guid CustomerId,
    Guid ServiceId,
    Guid ResourceId,
    Guid LocationId,
    DateTimeOffset StartUtc) : IRequest<Guid>;
