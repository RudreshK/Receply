using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>
/// A long-lived credential exchanged for a fresh short-lived JWT access token, so a staff member
/// doesn't have to re-enter an OTP every time the access token expires. Not tenant-owned, for the
/// same reason as LoginOtp - it's looked up by its hash alone, before any tenant is known. Only the
/// hash is ever stored; the raw token is returned to the client once, at issue time.
/// </summary>
public class RefreshToken : Entity
{
    public Guid StaffId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? RevokedAtUtc { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(Guid staffId, string tokenHash, TimeSpan validFor)
    {
        return new RefreshToken
        {
            StaffId = staffId,
            TokenHash = tokenHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(validFor)
        };
    }

    public bool IsUsable(DateTimeOffset nowUtc) => RevokedAtUtc is null && nowUtc < ExpiresAtUtc;

    /// <summary>Revoked either because it was rotated (a new one was issued in its place) or because
    /// the staff member explicitly signed out.</summary>
    public void Revoke() => RevokedAtUtc = DateTimeOffset.UtcNow;
}
