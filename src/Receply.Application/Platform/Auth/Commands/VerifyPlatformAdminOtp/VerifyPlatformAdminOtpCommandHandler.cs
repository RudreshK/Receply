using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Auth;
using Receply.Application.Common;

namespace Receply.Application.Platform.Auth.Commands.VerifyPlatformAdminOtp;

public class VerifyPlatformAdminOtpCommandHandler(IApplicationDbContext db, IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<VerifyPlatformAdminOtpCommand, PlatformAdminLoginResult>
{
    public async Task<PlatformAdminLoginResult> Handle(VerifyPlatformAdminOtpCommand request, CancellationToken cancellationToken)
    {
        var admin = await db.PlatformAdmins
            .FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("No active platform admin account found for that phone number.");

        var now = DateTimeOffset.UtcNow;

        var usableOtps = (await db.PlatformAdminLoginOtps
                .Where(o => o.PlatformAdminId == admin.Id)
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

        return new PlatformAdminLoginResult(tokenGenerator.GenerateToken(admin), admin.Id, admin.FullName);
    }
}
