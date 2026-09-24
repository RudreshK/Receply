using MediatR;

namespace Receply.Application.Auth.Commands.RequestOtp;

/// <summary>Returns the generated code so the Api layer can optionally echo it back in Development only - the handler itself stays environment-agnostic.</summary>
public record RequestOtpCommand(string PhoneNumber) : IRequest<string>;
