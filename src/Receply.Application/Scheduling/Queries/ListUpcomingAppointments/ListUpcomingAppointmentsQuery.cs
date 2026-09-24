using MediatR;

namespace Receply.Application.Scheduling.Queries.ListUpcomingAppointments;

public record UpcomingAppointmentDto(Guid AppointmentId, string ServiceName, DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Status);

public record ListUpcomingAppointmentsQuery(Guid TenantId, Guid ClientId) : IRequest<IReadOnlyList<UpcomingAppointmentDto>>;
