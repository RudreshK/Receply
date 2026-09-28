using MediatR;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features.Queries.GetTenantFeatures;

/// <summary>Every gateable Feature for the tenant, resolved to its effective on/off value - what the
/// dashboard reads once at login to decide which UI to show (the reply box, the Translate button, …).</summary>
public record GetTenantFeaturesQuery(Guid TenantId) : IRequest<Dictionary<Feature, bool>>;
