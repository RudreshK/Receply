using FluentValidation;

namespace Receply.Application.Platform.Commands.ConvertEnquiry;

public class ConvertEnquiryCommandValidator : AbstractValidator<ConvertEnquiryCommand>
{
    public ConvertEnquiryCommandValidator()
    {
        RuleFor(x => x.EnquiryId).NotEmpty();
        RuleFor(x => x.TimeZoneId).NotEmpty().MaximumLength(100);
    }
}
