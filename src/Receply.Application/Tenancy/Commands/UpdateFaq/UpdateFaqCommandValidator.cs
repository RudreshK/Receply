using FluentValidation;

namespace Receply.Application.Tenancy.Commands.UpdateFaq;

public class UpdateFaqCommandValidator : AbstractValidator<UpdateFaqCommand>
{
    public UpdateFaqCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.FaqId).NotEmpty();
        RuleFor(x => x.Question).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Answer).NotEmpty().MaximumLength(2000);
    }
}
