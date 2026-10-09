using MediatR;

namespace Receply.Application.Tenancy.Queries.GetSubscriptionStatus;

public record PlanOptionDto(string Plan, IReadOnlyList<string> Features);

public record SubscriptionStatusDto(
    string Plan,
    string Status,
    bool HasActiveAccess,
    DateTimeOffset? TrialEndsUtc,
    DateTimeOffset? PlanRenewsUtc,
    IReadOnlyList<PlanOptionDto> AvailablePlans);

public record GetSubscriptionStatusQuery(Guid TenantId) : IRequest<SubscriptionStatusDto>;
