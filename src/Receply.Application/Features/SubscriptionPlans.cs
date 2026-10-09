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

            // The essentials: a human can still take over a chat and see read receipts. No automation extras.
            [SubscriptionPlan.Basic] =
            [
                Feature.ManualStaffReply,
                Feature.ReadReceipts
            ],

            [SubscriptionPlan.Premium] = Enum.GetValues<Feature>()
        };

    public static IReadOnlyCollection<Feature> FeaturesFor(SubscriptionPlan plan) =>
        FeaturesByPlan.TryGetValue(plan, out var features) ? features : [];

    public static bool Includes(SubscriptionPlan plan, Feature feature) => FeaturesFor(plan).Contains(feature);
}
