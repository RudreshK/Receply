using MediatR;

namespace Receply.Application.Platform.Auth.Commands.VerifyPlatformAdminOtp;

public record PlatformAdminLoginResult(string Token, Guid PlatformAdminId, string FullName);

public record VerifyPlatformAdminOtpCommand(string PhoneNumber, string Code) : IRequest<PlatformAdminLoginResult>;
