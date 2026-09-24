using MediatR;

namespace Receply.Application.Scheduling.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid TenantId,
    Guid ClientId,
    Guid ServiceId,
    Guid ResourceId,
    Guid BranchId,
    DateTimeOffset StartUtc) : IRequest<Guid>;
