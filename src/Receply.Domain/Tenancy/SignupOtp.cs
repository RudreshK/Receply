using Receply.Domain.Common;

namespace Receply.Domain.Tenancy;

/// <summary>
/// A one-time code issued to a phone number during self-serve signup, before any Staff or Tenant
/// exists yet - keyed by phone number rather than StaffId (compare LoginOtp).
/// </summary>
public class SignupOtp : Entity
{
    private const int MaxAttempts = 5;

    public string PhoneNumber { get; private set; } = default!;
    public string CodeHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? ConsumedAtUtc { get; private set; }
    public int AttemptCount { get; private set; }

    private SignupOtp() { }

    public static SignupOtp Create(string phoneNumber, string codeHash, TimeSpan validFor)
    {
        return new SignupOtp
        {
            PhoneNumber = phoneNumber,
            CodeHash = codeHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(validFor)
        };
    }

    public bool IsUsable(DateTimeOffset nowUtc) =>
        ConsumedAtUtc is null && nowUtc < ExpiresAtUtc && AttemptCount < MaxAttempts;

    public void RecordFailedAttempt() => AttemptCount++;

    public void Consume() => ConsumedAtUtc = DateTimeOffset.UtcNow;
}
