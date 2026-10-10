using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Queries.GetBranding;

public class GetBrandingQueryHandler(IApplicationDbContext db) : IRequestHandler<GetBrandingQuery, BrandingDto>
{
    public async Task<BrandingDto> Handle(GetBrandingQuery request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        return new BrandingDto(
            tenant.Id,
            tenant.Name,
            tenant.BusinessEmail,
            tenant.Website,
            tenant.LogoDataUrl,
            tenant.OwnerName,
            tenant.Designation,
            tenant.OwnerPhotoDataUrl);
    }
}
