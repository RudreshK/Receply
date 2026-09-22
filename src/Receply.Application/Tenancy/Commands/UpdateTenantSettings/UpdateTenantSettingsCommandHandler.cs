using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.UpdateTenantSettings;

public class UpdateTenantSettingsCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateTenantSettingsCommand>
{
    public async Task Handle(UpdateTenantSettingsCommand request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        if (!Enum.TryParse<BusinessType>(request.BusinessType, ignoreCase: true, out var businessType))
            throw new ArgumentException($"Unknown business type '{request.BusinessType}'.", nameof(request.BusinessType));

        tenant.UpdateProfile(request.Name, businessType, request.TimeZoneId);

        await db.SaveChangesAsync(cancellationToken);
    }
}
