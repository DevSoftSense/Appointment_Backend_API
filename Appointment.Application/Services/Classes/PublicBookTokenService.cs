using System.Security.Cryptography;
using System.Text;
using Appointment.Application.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

/// <summary>
/// AES-GCM encrypted public book tokens. Payload: "orgId:appId".
/// Token is opaque Base64Url — customers cannot read or forge an org id.
/// </summary>
public sealed class PublicBookTokenService : IPublicBookTokenService
{
    private readonly byte[] _key;
    private readonly int _appId;

    public PublicBookTokenService(IConfiguration configuration)
    {
        _appId = configuration.GetValue<int?>("Appointment:AppId") ?? 25;
        var secret = configuration["Appointment:PublicBookTokenSecret"]
            ?? configuration["SoftOnCloud:Jwt:Secret"]
            ?? throw new InvalidOperationException("Appointment:PublicBookTokenSecret is not configured.");

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret + "|public-book-v1"));
    }

    public string CreateToken(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("orgId must be > 0.", nameof(orgId));

        var plain = Encoding.UTF8.GetBytes($"{orgId}:{_appId}");
        var nonce = RandomNumberGenerator.GetBytes(12);
        var cipher = new byte[plain.Length];
        var tag = new byte[16];

        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plain, cipher, tag);

        var blob = new byte[nonce.Length + cipher.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, blob, 0, nonce.Length);
        Buffer.BlockCopy(cipher, 0, blob, nonce.Length, cipher.Length);
        Buffer.BlockCopy(tag, 0, blob, nonce.Length + cipher.Length, tag.Length);

        return Base64UrlEncode(blob);
    }

    public bool TryResolve(string? token, out int orgId)
    {
        orgId = 0;
        if (string.IsNullOrWhiteSpace(token))
            return false;

        byte[] blob;
        try
        {
            blob = Base64UrlDecode(token.Trim());
        }
        catch
        {
            return false;
        }

        // nonce(12) + at least 1 byte cipher + tag(16)
        if (blob.Length < 12 + 1 + 16)
            return false;

        var nonce = blob.AsSpan(0, 12);
        var tag = blob.AsSpan(blob.Length - 16, 16);
        var cipher = blob.AsSpan(12, blob.Length - 12 - 16);
        var plain = new byte[cipher.Length];

        try
        {
            using var aes = new AesGcm(_key, 16);
            aes.Decrypt(nonce, cipher, tag, plain);
        }
        catch (CryptographicException)
        {
            return false;
        }

        var text = Encoding.UTF8.GetString(plain);
        var parts = text.Split(':');
        if (parts.Length != 2)
            return false;
        if (!int.TryParse(parts[0], out var resolvedOrg) || resolvedOrg <= 0)
            return false;
        if (!int.TryParse(parts[1], out var resolvedApp) || resolvedApp != _appId)
            return false;

        orgId = resolvedOrg;
        return true;
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
