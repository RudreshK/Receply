using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Auth.Commands.VerifyOtp;
using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Auth.Commands.CompleteSignup;

public class CompleteSignupCommandHandler(IApplicationDbContext db, IJwtTokenGenerator tokenGenerator)
    : IRequestHandler<CompleteSignupCommand, VerifyOtpResult>
{
    public async Task<VerifyOtpResult> Handle(CompleteSignupCommand request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var otp = await db.SignupOtps
            .Where(o => o.PhoneNumber == request.PhoneNumber)
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

        var token = tokenGenerator.GenerateToken(owner);
        return new VerifyOtpResult(token, tenant.Id, owner.Id, owner.FullName, owner.Role.ToString());
    }

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));
}
