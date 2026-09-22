using MediatR;

namespace Receply.Application.Scheduling.Queries.GetAppointmentDetail;

public record AppointmentDetailDto(
    Guid AppointmentId, Guid ClientId, string ClientName, string ClientPhoneNumber,
    Guid ServiceId, string ServiceName, Guid ResourceId, string ResourceName, Guid BranchId, string BranchName,
    DateTimeOffset StartUtc, DateTimeOffset EndUtc, string Status, string? CancellationReason);

public record GetAppointmentDetailQuery(Guid TenantId, Guid AppointmentId) : IRequest<AppointmentDetailDto>;
