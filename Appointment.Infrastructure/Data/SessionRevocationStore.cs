using Microsoft.Extensions.Caching.Memory;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// JWTs with iat &lt; revokeUnixSeconds are rejected.
/// Uses Unix seconds (same as JWT iat) so silent re-login in the same second stays valid.
/// </summary>
public sealed class SessionRevocationStore
{
    private readonly IMemoryCache _cache;

    public SessionRevocationStore(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void RevokeUser(int orgId, int userId, DateTimeOffset revokedAtUtc)
    {
        if (orgId <= 0 || userId <= 0) return;
        SetRevokeWatermark(orgId, userId, revokedAtUtc.ToUniversalTime().ToUnixTimeSeconds());
    }

    /// <summary>
    /// After password-change re-login: keep older tokens revoked, allow this JWT (iat &gt;= watermark).
    /// </summary>
    public void SetRevokeWatermark(int orgId, int userId, long revokeUnixSeconds)
    {
        if (orgId <= 0 || userId <= 0 || revokeUnixSeconds <= 0) return;
        _cache.Set(
            CacheKey(orgId, userId),
            revokeUnixSeconds,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });
    }

    public bool IsTokenRevoked(int orgId, int userId, DateTimeOffset? tokenIssuedAtUtc)
    {
        if (orgId <= 0 || userId <= 0) return false;
        if (!_cache.TryGetValue(CacheKey(orgId, userId), out long revokeUnix))
            return false;

        // Missing iat must NOT lock the user out (JwtBearer often omits iat from claims).
        // Caller should pass iat decoded from the raw Bearer JWT when possible.
        if (tokenIssuedAtUtc is null)
            return false;

        var tokenUnix = tokenIssuedAtUtc.Value.ToUniversalTime().ToUnixTimeSeconds();
        // Equal second is allowed so silent re-login in the same second stays valid.
        return tokenUnix < revokeUnix;
    }

    /// <summary>Clear revoke so a fresh login is never blocked by a prior logout watermark.</summary>
    public void ClearRevocation(int orgId, int userId)
    {
        if (orgId <= 0 || userId <= 0) return;
        _cache.Remove(CacheKey(orgId, userId));
    }

    private static string CacheKey(int orgId, int userId) =>
        $"appt:logout-revoke:{orgId}:{userId}";
}
