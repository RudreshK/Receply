using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.GetTenantSettings;

public class GetTenantSettingsQueryHandler(IApplicationDbContext db) : IRequestHandler<GetTenantSettingsQuery, TenantSettingsDto>
{
    public async Task<TenantSettingsDto> Handle(GetTenantSettingsQuery request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        return new TenantSettingsDto(tenant.Id, tenant.Name, tenant.BusinessType.ToString(), tenant.TimeZoneId, tenant.Status.ToString());
    }
}
