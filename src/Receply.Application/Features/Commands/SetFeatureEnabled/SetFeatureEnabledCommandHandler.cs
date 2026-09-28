using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features.Commands.SetFeatureEnabled;

public class SetFeatureEnabledCommandHandler(IApplicationDbContext db) : IRequestHandler<SetFeatureEnabledCommand>
{
    public async Task Handle(SetFeatureEnabledCommand request, CancellationToken cancellationToken)
    {
        var flag = await db.TenantFeatureFlags.FirstOrDefaultAsync(
            f => f.TenantId == request.TenantId && f.Feature == request.Feature, cancellationToken);

        if (flag is null)
        {
            db.TenantFeatureFlags.Add(TenantFeatureFlag.Create(request.TenantId, request.Feature, request.IsEnabled));
        }
        else
        {
            flag.SetEnabled(request.IsEnabled);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
