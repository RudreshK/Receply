using MediatR;
using Receply.Application.Auth.Commands.VerifyOtp;

namespace Receply.Application.Auth.Commands.CompleteSignup;

public record CompleteSignupCommand(
    string PhoneNumber,
    string Code,
    string BusinessName,
    string BusinessType,
    string TimeZoneId,
    string OwnerFirstName,
    string OwnerLastName,
    string OwnerEmail) : IRequest<VerifyOtpResult>;
