using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Auth.Commands.RequestSignupOtp;

public class RequestSignupOtpCommandHandler(IApplicationDbContext db, IOtpSender otpSender)
    : IRequestHandler<RequestSignupOtpCommand, string>
{
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);

    public async Task<string> Handle(RequestSignupOtpCommand request, CancellationToken cancellationToken)
    {
        var alreadyRegistered = await db.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (alreadyRegistered)
            throw new InvalidOperationException("An account with this phone number already exists - sign in instead.");

        var code = GenerateCode();
        var codeHash = Hash(code);

        db.SignupOtps.Add(SignupOtp.Create(request.PhoneNumber, codeHash, OtpValidity));
        await db.SaveChangesAsync(cancellationToken);

        await otpSender.SendAsync(request.PhoneNumber, code, cancellationToken);

        return code;
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
