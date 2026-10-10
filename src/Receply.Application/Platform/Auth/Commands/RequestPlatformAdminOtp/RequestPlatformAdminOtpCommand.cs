using MediatR;

namespace Receply.Application.Platform.Auth.Commands.RequestPlatformAdminOtp;

public record RequestPlatformAdminOtpCommand(string PhoneNumber) : IRequest<string>;
