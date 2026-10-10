using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.ListStaff;

public class ListStaffQueryHandler(IApplicationDbContext db) : IRequestHandler<ListStaffQuery, List<StaffListItemDto>>
{
    public async Task<List<StaffListItemDto>> Handle(ListStaffQuery request, CancellationToken cancellationToken)
    {
        return await db.Staff
            .Where(s => s.TenantId == request.TenantId)
            .OrderBy(s => s.Role)
            .ThenBy(s => s.FullName)
            .Select(s => new StaffListItemDto(s.Id, s.FullName, s.Email, s.PhoneNumber, s.Role.ToString(), s.IsActive))
            .ToListAsync(cancellationToken);
    }
}
