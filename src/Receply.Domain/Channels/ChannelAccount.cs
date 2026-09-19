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

    public void Deactivate() => IsActive = false;
}
