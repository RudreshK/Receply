namespace Receply.Infrastructure.Security;

public class EncryptionOptions
{
    public const string SectionName = "Encryption";

    /// <summary>Base64-encoded 32-byte (AES-256) key. Must be set to the identical value on every
    /// service that reads or writes a ChannelAccount credential (Api and Workers both send WhatsApp
    /// messages) - a token encrypted by one has to be decryptable by the other.</summary>
    public string Key { get; set; } = default!;
}
