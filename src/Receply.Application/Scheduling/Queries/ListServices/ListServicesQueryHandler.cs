using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Scheduling.Queries.ListServices;

public class ListServicesQueryHandler(IApplicationDbContext db) : IRequestHandler<ListServicesQuery, IReadOnlyList<ServiceDto>>
{
    public async Task<IReadOnlyList<ServiceDto>> Handle(ListServicesQuery request, CancellationToken cancellationToken)
    {
        return await db.Services
            .Where(s => s.TenantId == request.TenantId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new ServiceDto(s.Id, s.Name, s.Duration, s.Price))
            .ToListAsync(cancellationToken);
    }
}
