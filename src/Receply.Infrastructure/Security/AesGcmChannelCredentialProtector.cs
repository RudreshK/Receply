using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Receply.Application.Channels;

namespace Receply.Infrastructure.Security;

/// <summary>
/// AES-256-GCM, not the ASP.NET Core Data Protection API - Data Protection's default key ring is
/// per-machine/per-app-service local storage, which Api and Workers (separate Azure App Services)
/// don't share. A single configured key, identical on both, keeps this simple without standing up
/// shared Blob Storage/Key Vault infrastructure for it.
/// </summary>
public class AesGcmChannelCredentialProtector : IChannelCredentialProtector
{
    private const int NonceSizeBytes = 12;
    private const int TagSizeBytes = 16;

    private readonly byte[] _key;

    public AesGcmChannelCredentialProtector(IOptions<EncryptionOptions> options)
    {
        var keyBase64 = options.Value.Key;
        if (string.IsNullOrWhiteSpace(keyBase64))
            throw new InvalidOperationException("Encryption:Key is not configured.");

        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption:Key must be a base64-encoded 32-byte (AES-256) key.");
    }

    public string Protect(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSizeBytes);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSizeBytes];

        using (var aesGcm = new AesGcm(_key, TagSizeBytes))
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var packed = new byte[NonceSizeBytes + TagSizeBytes + ciphertext.Length];
        Buffer.BlockCopy(nonce, 0, packed, 0, NonceSizeBytes);
        Buffer.BlockCopy(tag, 0, packed, NonceSizeBytes, TagSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, packed, NonceSizeBytes + TagSizeBytes, ciphertext.Length);

        return Convert.ToBase64String(packed);
    }

    public string Unprotect(string ciphertext)
    {
        var packed = Convert.FromBase64String(ciphertext);
        var nonce = packed.AsSpan(0, NonceSizeBytes);
        var tag = packed.AsSpan(NonceSizeBytes, TagSizeBytes);
        var cipherBytes = packed.AsSpan(NonceSizeBytes + TagSizeBytes);

        var plaintextBytes = new byte[cipherBytes.Length];
        using (var aesGcm = new AesGcm(_key, TagSizeBytes))
            aesGcm.Decrypt(nonce, cipherBytes, tag, plaintextBytes);

        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
