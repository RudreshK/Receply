using System.Security.Cryptography;
using System.Text;

namespace Receply.Application.Auth;

/// <summary>Shared hashing for anything we store only the hash of and match by (OTP codes, refresh
/// tokens) - never the raw secret.</summary>
public static class TokenHasher
{
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    /// <summary>A cryptographically random opaque token, safe to hand to a client as a refresh token.</summary>
    public static string GenerateRawToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .Replace('+', '-').Replace('/', '_').TrimEnd('=');
}
