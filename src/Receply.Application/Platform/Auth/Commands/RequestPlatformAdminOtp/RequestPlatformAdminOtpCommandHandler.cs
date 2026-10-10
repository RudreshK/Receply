using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Auth;
using Receply.Application.Common;
using Receply.Domain.Platform;

namespace Receply.Application.Platform.Auth.Commands.RequestPlatformAdminOtp;

public class RequestPlatformAdminOtpCommandHandler(IApplicationDbContext db, IOtpSender otpSender, IEmailSender emailSender)
    : IRequestHandler<RequestPlatformAdminOtpCommand, string>
{
    private static readonly TimeSpan OtpValidity = TimeSpan.FromMinutes(5);

    public async Task<string> Handle(RequestPlatformAdminOtpCommand request, CancellationToken cancellationToken)
    {
        var admin = await db.PlatformAdmins
            .FirstOrDefaultAsync(a => a.PhoneNumber == request.PhoneNumber && a.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("No active platform admin account found for that phone number.");

        var code = GenerateCode();
        var codeHash = TokenHasher.Hash(code);

        db.PlatformAdminLoginOtps.Add(PlatformAdminLoginOtp.Create(admin.Id, codeHash, OtpValidity));
        await db.SaveChangesAsync(cancellationToken);

        await otpSender.SendAsync(admin.PhoneNumber, code, cancellationToken);

        if (!string.IsNullOrWhiteSpace(admin.Email))
        {
            await emailSender.SendAsync(
                admin.Email,
                "Your Receply admin login code",
                $"Your login code is {code}. It expires in 5 minutes.",
                cancellationToken);
        }

        return code;
    }

    private static string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
}
