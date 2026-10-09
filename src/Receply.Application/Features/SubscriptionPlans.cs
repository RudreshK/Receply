using Receply.Domain.Tenancy;

namespace Receply.Application.Features;

/// <summary>What each subscription tier includes - the default a tenant's features fall back to
/// before any per-tenant TenantFeatureFlag override is considered. Edit this to change what a plan
/// is worth; it's the one place that decision lives.</summary>
public static class SubscriptionPlans
{
    private static readonly IReadOnlyDictionary<SubscriptionPlan, IReadOnlyCollection<Feature>> FeaturesByPlan =
        new Dictionary<SubscriptionPlan, IReadOnlyCollection<Feature>>
        {
            // Trial gets the full product, so a prospective customer sees everything it's worth paying for.
            [SubscriptionPlan.Trial] = Enum.GetValues<Feature>(),

            // "AI answers common questions, business hours, location, and services" - informational
            // only. No appointment booking (that's the Growth upsell) and no automation extras.
            [SubscriptionPlan.Starter] =
            [
                Feature.ManualStaffReply,
                Feature.ReadReceipts
            ],

            // The flagship plan: everything, including appointment booking, reminders, and the
            // performance dashboard.
            [SubscriptionPlan.Growth] = Enum.GetValues<Feature>(),

            // Same feature set as Growth - what Assisted Setup adds (personal onboarding, priority
            // support) is a service, not a toggleable capability, so there's nothing to gate here.
            [SubscriptionPlan.AssistedSetup] = Enum.GetValues<Feature>()
        };

    public static IReadOnlyCollection<Feature> FeaturesFor(SubscriptionPlan plan) =>
        FeaturesByPlan.TryGetValue(plan, out var features) ? features : [];

    public static bool Includes(SubscriptionPlan plan, Feature feature) => FeaturesFor(plan).Contains(feature);

    public static PlanPricing PricingFor(SubscriptionPlan plan) => plan switch
    {
        SubscriptionPlan.Trial => new PlanPricing("Free Trial", "Free for 14 days", "Try the full product before you commit"),
        SubscriptionPlan.Starter => new PlanPricing("Starter", "₹999/mo", "For small, single-location businesses"),
        SubscriptionPlan.Growth => new PlanPricing("Growth", "₹1,999/mo", "Your main plan"),
        SubscriptionPlan.AssistedSetup => new PlanPricing("Assisted Setup", "₹2,999+/mo", "For owners who want us to configure everything"),
        _ => new PlanPricing(plan.ToString(), "", "")
    };
}

public record PlanPricing(string DisplayName, string PriceLabel, string Description);
