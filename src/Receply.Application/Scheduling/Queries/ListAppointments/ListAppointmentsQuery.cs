using MediatR;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Queries.ListAppointments;

public record AppointmentListItemDto(
    Guid AppointmentId, string ClientName, string ServiceName, string ResourceName, Guid BranchId,
    DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Status);

public record ListAppointmentsQuery(
    Guid TenantId, DateTimeOffset From, DateTimeOffset To, Guid? BranchId, AppointmentStatus? Status)
    : IRequest<List<AppointmentListItemDto>>;
