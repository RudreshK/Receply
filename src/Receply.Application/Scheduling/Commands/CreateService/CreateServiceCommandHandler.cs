using MediatR;
using Receply.Application.Common;
using Receply.Domain.Scheduling;

namespace Receply.Application.Scheduling.Commands.CreateService;

public class CreateServiceCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateServiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
    {
        var service = Service.Create(request.TenantId, request.Name, TimeSpan.FromMinutes(request.DurationMinutes), request.Price);
        db.Services.Add(service);
        await db.SaveChangesAsync(cancellationToken);
        return service.Id;
    }
}
