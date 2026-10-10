using FluentValidation;

namespace Receply.Application.Platform.Commands.CreateEnquiry;

public class CreateEnquiryCommandValidator : AbstractValidator<CreateEnquiryCommand>
{
    public CreateEnquiryCommandValidator()
    {
        RuleFor(x => x.BusinessName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BusinessType).NotEmpty();
        RuleFor(x => x.ContactName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Email).MaximumLength(320);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}
