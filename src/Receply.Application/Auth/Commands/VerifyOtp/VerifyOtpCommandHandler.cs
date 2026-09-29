using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Common;

namespace Receply.Application.Auth.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(IApplicationDbContext db, AccessTokenIssuer tokenIssuer)
    : IRequestHandler<VerifyOtpCommand, VerifyOtpResult>
{
    public async Task<VerifyOtpResult> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var staff = await db.Staff
            .FirstOrDefaultAsync(s => s.PhoneNumber == request.PhoneNumber && s.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("No active staff account found for that phone number.");

        var now = DateTimeOffset.UtcNow;

        // Checked against every still-usable code for this staff member, not just the latest one -
        // requesting a code more than once must not silently invalidate an earlier email they're still reading.
        var usableOtps = (await db.LoginOtps
                .Where(o => o.StaffId == staff.Id)
                .OrderByDescending(o => o.CreatedOn)
                .ToListAsync(cancellationToken))
            .Where(o => o.IsUsable(now))
            .ToList();

        if (usableOtps.Count == 0)
            throw new InvalidOperationException("No valid code found - request a new one.");

        var codeHash = TokenHasher.Hash(request.Code);
        var otp = usableOtps.FirstOrDefault(o => o.CodeHash == codeHash);
        if (otp is null)
        {
            usableOtps[0].RecordFailedAttempt();
            await db.SaveChangesAsync(cancellationToken);
            throw new InvalidOperationException("Incorrect code.");
        }

        otp.Consume();
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, refreshToken) = await tokenIssuer.IssueAsync(staff, cancellationToken);
        return new VerifyOtpResult(accessToken, refreshToken, staff.TenantId, staff.Id, staff.FullName, staff.Role.ToString(), staff.PreferredLanguage);
    }
}
