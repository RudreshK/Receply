using MediatR;

namespace Receply.Application.Tenancy.Commands.ChangeSubscriptionPlan;

/// <summary>Self-serve plan switch from the Settings page. There's no payment gateway behind this
/// yet, so this is effectively an honor-system pick - the business owner chooses Basic or Premium and
/// pays offline; a real gateway would call this same command from its webhook once one exists.</summary>
public record ChangeSubscriptionPlanCommand(Guid TenantId, string Plan) : IRequest;
