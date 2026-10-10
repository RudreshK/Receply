using FluentValidation;

namespace Receply.Application.Scheduling.Commands.CreateService;

public class CreateServiceCommandValidator : AbstractValidator<CreateServiceCommand>
{
    public CreateServiceCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
