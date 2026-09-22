using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>
/// A one-time login code issued to a Staff member's phone number. Not tenant-owned: login happens
/// before any tenant is resolved, so lookup is by StaffId/PhoneNumber only.
/// </summary>
public class LoginOtp : Entity
{
    private const int MaxAttempts = 5;

    public Guid StaffId { get; private set; }
    public string CodeHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? ConsumedAtUtc { get; private set; }
    public int AttemptCount { get; private set; }

    private LoginOtp() { }

    public static LoginOtp Create(Guid staffId, string codeHash, TimeSpan validFor)
    {
        return new LoginOtp
        {
            StaffId = staffId,
            CodeHash = codeHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(validFor)
        };
    }

    public bool IsUsable(DateTimeOffset nowUtc) =>
        ConsumedAtUtc is null && nowUtc < ExpiresAtUtc && AttemptCount < MaxAttempts;

    public void RecordFailedAttempt() => AttemptCount++;

    public void Consume() => ConsumedAtUtc = DateTimeOffset.UtcNow;
}
