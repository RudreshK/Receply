using MediatR;

namespace Receply.Application.Scheduling.Queries.GetAvailableSlots;

public record AvailableSlotDto(Guid ResourceId, string ResourceName, DateTimeOffset StartUtc, DateTimeOffset EndUtc);

public record GetAvailableSlotsQuery(Guid TenantId, string ServiceName, DateOnly Date) : IRequest<IReadOnlyList<AvailableSlotDto>>;
