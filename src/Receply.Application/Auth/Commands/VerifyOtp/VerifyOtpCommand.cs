using MediatR;

namespace Receply.Application.Auth.Commands.VerifyOtp;

public record VerifyOtpResult(string Token, Guid TenantId, Guid StaffId, string FullName, string Role);

public record VerifyOtpCommand(string PhoneNumber, string Code) : IRequest<VerifyOtpResult>;
