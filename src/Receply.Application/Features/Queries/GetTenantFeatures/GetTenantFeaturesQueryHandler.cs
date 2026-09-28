using MediatR;
using Receply.Domain.Tenancy;

namespace Receply.Application.Features.Queries.GetTenantFeatures;

public class GetTenantFeaturesQueryHandler(IFeatureAccess featureAccess) : IRequestHandler<GetTenantFeaturesQuery, Dictionary<Feature, bool>>
{
    public async Task<Dictionary<Feature, bool>> Handle(GetTenantFeaturesQuery request, CancellationToken cancellationToken)
    {
        var result = new Dictionary<Feature, bool>();

        foreach (var feature in Enum.GetValues<Feature>())
            result[feature] = await featureAccess.IsEnabledAsync(request.TenantId, feature, cancellationToken);

        return result;
    }
}
