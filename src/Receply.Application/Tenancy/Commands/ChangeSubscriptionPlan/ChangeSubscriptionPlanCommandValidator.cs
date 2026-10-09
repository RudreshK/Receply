using FluentValidation;
using Receply.Domain.Tenancy;

namespace Receply.Application.Tenancy.Commands.ChangeSubscriptionPlan;

public class ChangeSubscriptionPlanCommandValidator : AbstractValidator<ChangeSubscriptionPlanCommand>
{
    public ChangeSubscriptionPlanCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();

        // Trial isn't something you can pick your way back into - it's only ever the state a new
        // tenant starts in.
        RuleFor(x => x.Plan)
            .Must(p => Enum.TryParse<SubscriptionPlan>(p, ignoreCase: true, out var plan) && plan != SubscriptionPlan.Trial)
            .WithMessage("Plan must be Basic or Premium.");
    }
}
