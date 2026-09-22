using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Auth.Commands.RequestOtp;

public class RequestOtpCommandHandler(IApplicationDbContext db, IOtpSender otpSender) : IRequestHandler<RequestOtpCommand, string>
{
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);

    public async Task<string> Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        // No tenant is resolved yet at this point (anonymous request, no JWT), so the ambient
        // tenant query filter is a no-op here and this naturally searches across all tenants.
        var staff = await db.Staff
            .FirstOrDefaultAsync(s => s.PhoneNumber == request.PhoneNumber && s.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("No active staff account found for that phone number.");

        var code = GenerateCode();
        var codeHash = Hash(code);

        db.LoginOtps.Add(LoginOtp.Create(staff.Id, codeHash, OtpValidity));
        await db.SaveChangesAsync(cancellationToken);

        await otpSender.SendAsync(staff.PhoneNumber, code, cancellationToken);

        return code;
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");

    private static string Hash(string code) =>
        Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code)));
}
