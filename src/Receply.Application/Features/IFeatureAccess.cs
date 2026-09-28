using Receply.Domain.Tenancy;

namespace Receply.Application.Features;

/// <summary>
/// The single seam every feature-gated command handler, controller, or webhook path checks before
/// doing anything a tenant's subscription might not include. Keeps "is this business allowed to use
/// X" out of the feature's own code entirely, so a feature can be enabled/disabled per tenant -
/// including remotely, e.g. from a future billing webhook - without touching it.
/// </summary>
public interface IFeatureAccess
{
    Task<bool> IsEnabledAsync(Guid tenantId, Feature feature, CancellationToken cancellationToken = default);
}
