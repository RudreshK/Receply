using FluentValidation;

namespace Receply.Application.Auth.Commands.CompleteSignup;

public class CompleteSignupCommandValidator : AbstractValidator<CompleteSignupCommand>
{
    public CompleteSignupCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Code).NotEmpty().Length(6);
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessType).NotEmpty();
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.OwnerFullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OwnerEmail).MaximumLength(320);
    }
}
