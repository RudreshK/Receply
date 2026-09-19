using MediatR;

namespace Receply.Application.Scheduling.Queries.ListServices;

public record ServiceDto(Guid Id, string Name, TimeSpan Duration, decimal Price);

public record ListServicesQuery(Guid TenantId) : IRequest<IReadOnlyList<ServiceDto>>;
