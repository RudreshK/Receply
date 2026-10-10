using MediatR;

namespace Receply.Application.Scheduling.Commands.CreateService;

public record CreateServiceCommand(Guid TenantId, string Name, int DurationMinutes, decimal Price) : IRequest<Guid>;
