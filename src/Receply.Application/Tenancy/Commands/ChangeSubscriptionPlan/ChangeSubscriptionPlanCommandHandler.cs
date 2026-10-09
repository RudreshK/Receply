using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.ChangeSubscriptionPlan;

public class ChangeSubscriptionPlanCommandHandler(IApplicationDbContext db) : IRequestHandler<ChangeSubscriptionPlanCommand>
{
    public async Task Handle(ChangeSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        var plan = Enum.Parse<SubscriptionPlan>(request.Plan, ignoreCase: true);

        // No billing cycle to drive a renewal date yet - a paid plan just doesn't expire on its own
        // until one does.
        tenant.ChangePlan(plan, renewsUtc: null);

        await db.SaveChangesAsync(cancellationToken);
    }
}
