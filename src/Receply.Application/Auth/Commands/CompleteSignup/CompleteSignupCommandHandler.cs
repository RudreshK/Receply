using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Auth.Commands.VerifyOtp;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Auth.Commands.CompleteSignup;

public class CompleteSignupCommandHandler(IApplicationDbContext db, AccessTokenIssuer tokenIssuer)
    : IRequestHandler<CompleteSignupCommand, VerifyOtpResult>
{
    public async Task<VerifyOtpResult> Handle(CompleteSignupCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        // Checked against every still-usable code for this phone number, not just the latest one -
        // requesting a code more than once (a double click, hitting "resend" while impatient, going
        // back and forward) must not silently invalidate an earlier email the person is still reading.
        var usableOtps = (await db.SignupOtps
                .Where(o => o.PhoneNumber == request.PhoneNumber)
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

        var alreadyRegistered = await db.Staff.AnyAsync(s => s.PhoneNumber == request.PhoneNumber, cancellationToken);
        if (alreadyRegistered)
            throw new InvalidOperationException("An account with this phone number already exists - sign in instead.");

        if (!Enum.TryParse<BusinessType>(request.BusinessType, ignoreCase: true, out var businessType))
            throw new ArgumentException($"Unknown business type '{request.BusinessType}'.", nameof(request.BusinessType));

        otp.Consume();

        var tenant = Tenant.Create(request.BusinessName, businessType, request.TimeZoneId);
        tenant.AddBranch("Main Branch", "");
        var owner = Staff.Create(tenant.Id, request.OwnerFullName, request.OwnerEmail ?? "", request.PhoneNumber, StaffRole.Owner);

        db.Tenants.Add(tenant);
        db.Staff.Add(owner);
        await db.SaveChangesAsync(cancellationToken);

        var (accessToken, refreshToken) = await tokenIssuer.IssueAsync(owner, cancellationToken);
        return new VerifyOtpResult(accessToken, refreshToken, tenant.Id, owner.Id, owner.FullName, owner.Role.ToString(), owner.PreferredLanguage);
    }
}
