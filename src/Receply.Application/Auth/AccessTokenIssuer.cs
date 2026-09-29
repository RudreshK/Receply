using Receply.Application.Common;
using Receply.Domain.Tenancy;

namespace Receply.Application.Auth;

/// <summary>
/// Issues a fresh (JWT access token, raw refresh token) pair for a staff member and persists the
/// refresh token's hash - the one place shared by every credential-issuing flow: OTP login, signup,
/// and refreshing an expiring access token. Saves changes itself, so callers that also mutated
/// something else beforehand (consuming an OTP, revoking the old refresh token) get that persisted
/// in the same call.
/// </summary>
public class AccessTokenIssuer(IApplicationDbContext db, IJwtTokenGenerator tokenGenerator)
{
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(30);

    public async Task<(string AccessToken, string RefreshToken)> IssueAsync(Staff staff, CancellationToken cancellationToken)
    {
        var rawRefreshToken = TokenHasher.GenerateRawToken();
        db.RefreshTokens.Add(RefreshToken.Create(staff.Id, TokenHasher.Hash(rawRefreshToken), RefreshTokenLifetime));
        await db.SaveChangesAsync(cancellationToken);

        return (tokenGenerator.GenerateToken(staff), rawRefreshToken);
    }
}
