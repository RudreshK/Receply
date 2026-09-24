using FluentValidation;

namespace Receply.Application.Auth.Commands.RequestOtp;

public class RequestOtpCommandValidator : AbstractValidator<RequestOtpCommand>
{
    public RequestOtpCommandValidator()
    {
        RuleFor(x => x.PhoneNumber).NotEmpty().MaximumLength(32);
    }
}
