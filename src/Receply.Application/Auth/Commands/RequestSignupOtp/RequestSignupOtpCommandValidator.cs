using FluentValidation;

namespace Receply.Application.Auth.Commands.RequestSignupOtp;

public class RequestSignupOtpCommandValidator : AbstractValidator<RequestSignupOtpCommand>
{
    public RequestSignupOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
