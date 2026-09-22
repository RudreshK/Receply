using MediatR;

namespace Receply.Application.Auth.Commands.RequestSignupOtp;

/// <summary>Returns the generated code so the Api layer can optionally echo it back in Development only - the handler itself stays environment-agnostic.</summary>
public record RequestSignupOtpCommand(string PhoneNumber) : IRequest<string>;
