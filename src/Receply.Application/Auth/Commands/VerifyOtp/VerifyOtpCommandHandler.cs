using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(IApplicationDbContext db, IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResult>
{
    public async Task<VerifyOtpResult> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff
            .FirstOrDefaultAsync(s => s.PhoneNumber == request.PhoneNumber && s.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("No active staff account found for that phone number.");

        var now = DateTimeOffset.UtcNow;
        var otp = await db.LoginOtps
            .Where(o => o.StaffId == staff.Id)
            .OrderByDescending(o => o.CreatedOn)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null || !otp.IsUsable(now))
            throw new InvalidOperationException("No valid code found - request a new one.");

        var codeHash = Hash(request.Code);
        if (codeHash != otp.CodeHash)
        {
            otp.RecordFailedAttempt();
            await db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Incorrect code.");
        }

        otp.Consume();
        await db.SaveChangesAsync(cancellationToken);

        var token = tokenGenerator.GenerateToken(staff);
        return new VerifyOtpResult(token, staff.TenantId, staff.Id, staff.FullName, staff.Role.ToString());
    }

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
