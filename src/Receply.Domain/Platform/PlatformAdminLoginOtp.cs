using Receply.Domain.Common;

namespace Receply.Domain.Platform;

/// <summary>A one-time login code issued to a PlatformAdmin's phone number - mirrors Tenancy.LoginOtp
/// exactly, kept separate because it's keyed to a PlatformAdmin, not a Staff member.</summary>
public class PlatformAdminLoginOtp : Entity
{
    private const int MaxAttempts = 5;

    public Guid PlatformAdminId { get; private set; }
    public string CodeHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public DateTimeOffset? ConsumedAtUtc { get; private set; }
    public int AttemptCount { get; private set; }

    private PlatformAdminLoginOtp() { }

    public static PlatformAdminLoginOtp Create(Guid platformAdminId, string codeHash, TimeSpan validFor)
    {
        return new PlatformAdminLoginOtp
        {
            PlatformAdminId = platformAdminId,
            CodeHash = codeHash,
            ExpiresAtUtc = DateTimeOffset.UtcNow.Add(validFor)
        };
    }

    public bool IsUsable(DateTimeOffset nowUtc) =>
        ConsumedAtUtc is null && nowUtc < ExpiresAtUtc && AttemptCount < MaxAttempts;

    public void RecordFailedAttempt() => AttemptCount++;

    public void Consume() => ConsumedAtUtc = DateTimeOffset.UtcNow;
}
