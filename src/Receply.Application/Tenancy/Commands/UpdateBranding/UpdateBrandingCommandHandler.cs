using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Tenancy.Commands.UpdateBranding;

public class UpdateBrandingCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateBrandingCommand>
{
    public async Task Handle(UpdateBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        tenant.UpdateBranding(
            request.Name,
            request.BusinessEmail,
            request.Website,
            request.LogoDataUrl,
            request.OwnerName,
            request.Designation,
            request.OwnerPhotoDataUrl);

        await db.SaveChangesAsync(cancellationToken);
    }
}
