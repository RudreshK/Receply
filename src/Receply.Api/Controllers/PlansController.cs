using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Receply.Application.Features;
using Receply.Application.Tenancy.Queries.GetSubscriptionStatus;
using Receply.Domain.Tenancy;

namespace Receply.Api.Controllers;

/// <summary>Public pricing, for the landing page - no tenant/auth involved, so prospects can see
/// what a plan costs and includes before signing up.</summary>
[AllowAnonymous]
[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    [HttpGet]
    public ActionResult<List<PlanOptionDto>> List()
    {
        var plans = Enum.GetValues<SubscriptionPlan>()
            .Where(p => p != SubscriptionPlan.Trial) // not something a visitor picks directly
            .Select(plan =>
            {
                var pricing = SubscriptionPlans.PricingFor(plan);
                return new PlanOptionDto(
                    plan.ToString(), pricing.DisplayName, pricing.PriceLabel, pricing.Description,
                    SubscriptionPlans.FeaturesFor(plan).Select(f => f.ToString()).ToList());
            })
            .ToList();

        return plans;
    }
}
