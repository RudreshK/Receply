using MediatR;

namespace Receply.Application.Auth.Commands.VerifyOtp;

public record VerifyOtpResult(string Token, string RefreshToken, Guid TenantId, Guid StaffId, string FullName, string Role, string? PreferredLanguage);

public record VerifyOtpCommand(string PhoneNumber, string Code) : IRequest<VerifyOtpResult>;
