namespace Receply.Application.Channels;

/// <summary>
/// Encrypts/decrypts a per-tenant channel credential (a WhatsApp access token, say) before it's
/// stored on or read from a ChannelAccount. Every write path for ChannelAccount.AccessToken must go
/// through Protect() first - the entity itself only ever holds ciphertext.
/// </summary>
public interface IChannelCredentialProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
