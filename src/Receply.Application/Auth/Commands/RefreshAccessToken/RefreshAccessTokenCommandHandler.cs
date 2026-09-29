using MediatR;
using Microsoft.EntityFrameworkCore;
using Receply.Application.Auth.Commands.VerifyOtp;
using Receply.Application.Common;

namespace Receply.Application.Auth.Commands.RefreshAccessToken;

public class RefreshAccessTokenCommandHandler(IApplicationDbContext db, AccessTokenIssuer tokenIssuer)
    : IRequestHandler<RefreshAccessTokenCommand, VerifyOtpResult>
{
    public async Task<VerifyOtpResult> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = TokenHasher.Hash(request.RefreshToken);
        var now = DateTimeOffset.UtcNow;

        var stored = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);
        if (stored is null || !stored.IsUsable(now))
            throw new InvalidOperationException("Refresh token is invalid or expired - sign in again.");

        var staff = await db.Staff.FirstOrDefaultAsync(s => s.Id == stored.StaffId && s.IsActive, cancellationToken)
            ?? throw new InvalidOperationException("Refresh token is invalid or expired - sign in again.");

        // Single-use: rotate on every refresh. A replayed/stolen token is caught the moment the
        // legitimate client tries to use its (now-revoked) copy again.
        stored.Revoke();

        var (accessToken, refreshToken) = await tokenIssuer.IssueAsync(staff, cancellationToken);
        return new VerifyOtpResult(accessToken, refreshToken, staff.TenantId, staff.Id, staff.FullName, staff.Role.ToString(), staff.PreferredLanguage);
    }
}
