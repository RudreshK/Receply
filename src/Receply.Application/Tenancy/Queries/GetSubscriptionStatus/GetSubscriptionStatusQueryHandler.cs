using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Application.Features;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Queries.GetSubscriptionStatus;

public class GetSubscriptionStatusQueryHandler(IApplicationDbContext db) : IRequestHandler<GetSubscriptionStatusQuery, SubscriptionStatusDto>
{
    public async Task<SubscriptionStatusDto> Handle(GetSubscriptionStatusQuery request, CancellationToken cancellationToken)
    {
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{request.TenantId}' not found.");

        var availablePlans = Enum.GetValues<SubscriptionPlan>()
            .Select(plan =>
            {
                var pricing = SubscriptionPlans.PricingFor(plan);
                return new PlanOptionDto(
                    plan.ToString(), pricing.DisplayName, pricing.PriceLabel, pricing.Description,
                    SubscriptionPlans.FeaturesFor(plan).Select(f => f.ToString()).ToList());
            })
            .ToList();

        return new SubscriptionStatusDto(
            tenant.Plan.ToString(),
            tenant.Status.ToString(),
            tenant.HasActiveAccess(DateTimeOffset.UtcNow),
            tenant.TrialEndsUtc,
            tenant.PlanRenewsUtc,
            availablePlans);
    }
}
