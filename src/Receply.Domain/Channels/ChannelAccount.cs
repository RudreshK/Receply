using Receply.Domain.Common;

namespace Receply.Domain.Channels;

/// <summary>
/// A tenant's connected channel (e.g. a WhatsApp Business phone number).
/// Provider credentials are stored encrypted; this entity only tracks identity/config.
/// </summary>
public class ChannelAccount : TenantOwnedEntity
{
    public ChannelType Type { get; private set; }
    public string ExternalId { get; private set; } = default!; // e.g. WhatsApp phone_number_id
    public string DisplayName { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    /// <summary>Ciphertext, opaque to the domain - IChannelCredentialProtector in Infrastructure is
    /// what encrypts this before it ever gets here and decrypts it right before use. Null means "use
    /// the platform's shared token" (today's only tenant, seeded before each tenant had its own).</summary>
    public string? AccessToken { get; private set; }

    /// <summary>WhatsApp Business Account id - not needed to send messages today, but every future
    /// WABA-level call (templates, number migration, Embedded Signup bookkeeping) will want it
    /// recorded per tenant from the start rather than backfilled later.</summary>
    public string? WabaId { get; private set; }

    private ChannelAccount() { }

    public static ChannelAccount Create(Guid tenantId, ChannelType type, string externalId, string displayName)
    {
        var account = new ChannelAccount
        {
            Type = type,
            ExternalId = externalId,
            DisplayName = displayName
        };
        account.TenantId = tenantId;
        return account;
    }

    /// <summary>Sets or rotates this account's own WhatsApp credentials. accessToken must already be
    /// encrypted by the caller (see IChannelCredentialProtector) - this entity never sees plaintext.</summary>
    public void SetCredentials(string? encryptedAccessToken, string? wabaId)
    {
        AccessToken = encryptedAccessToken;
        WabaId = wabaId;
    }

    public void Deactivate() => IsActive = false;
}
