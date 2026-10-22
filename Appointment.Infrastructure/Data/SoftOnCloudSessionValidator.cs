using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// SoftOnCloud v2.5 — validates that the JWT session is still alive (password change / revoke).
/// Cached briefly so Chrome↔Edge logout is ~1s without the browser polling SoftOnCloud.
/// </summary>
public sealed class SoftOnCloudSessionValidator
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SoftOnCloudSessionValidator> _logger;

    public SoftOnCloudSessionValidator(
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        IConfiguration configuration,
        ILogger<SoftOnCloudSessionValidator> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SoftOnCloudSessionCheckResult> ValidateBearerAsync(
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader)
            || !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return SoftOnCloudSessionCheckResult.Fail("SESSION_ENDED", "Missing SoftOnCloud bearer token.");
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
        {
            return SoftOnCloudSessionCheckResult.Fail("SESSION_ENDED", "Missing SoftOnCloud bearer token.");
        }

        var cacheKey = "soc:session:" + HashToken(token);
        if (_cache.TryGetValue(cacheKey, out SoftOnCloudSessionCheckResult? cached) && cached is not null)
        {
            return cached;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("SoftOnCloud");
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/session-status");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var ok = SoftOnCloudSessionCheckResult.Ok();
                var ttlSeconds = Math.Clamp(
                    _configuration.GetValue("SoftOnCloud:SessionValidationCacheSeconds", 1),
                    1,
                    30);
                _cache.Set(
                    cacheKey,
                    ok,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ttlSeconds)
                    });
                return ok;
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var code = "SESSION_ENDED";
                var message = "Your session has ended. Please sign in again.";
                try
                {
                    await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                    var body = await JsonSerializer.DeserializeAsync<SessionStatusErrorBody>(
                        stream,
                        JsonOptions,
                        cancellationToken);
                    var rawCode = (body?.Code ?? string.Empty).Trim().ToUpperInvariant();
                    if (rawCode is "PASSWORD_CHANGED")
                    {
                        code = "PASSWORD_CHANGED";
                        message = string.IsNullOrWhiteSpace(body?.Message)
                            ? "Your password was changed. Please sign in again."
                            : body!.Message!;
                    }
                    else if (!string.IsNullOrWhiteSpace(body?.Message))
                    {
                        message = body!.Message!;
                    }
                }
                catch
                {
                    /* keep defaults */
                }

                // Do not cache failures — next request must re-check SoftOnCloud immediately.
                return SoftOnCloudSessionCheckResult.Fail(code, message);
            }

            _logger.LogWarning(
                "SoftOnCloud session-status unexpected status {StatusCode}",
                (int)response.StatusCode);
            // SoftOnCloud hiccup — fail open so staff are not locked out of Appointment.
            return SoftOnCloudSessionCheckResult.Ok();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SoftOnCloud session-status probe failed; allowing request (fail-open).");
            return SoftOnCloudSessionCheckResult.Ok();
        }
    }

    private static string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }

    private sealed class SessionStatusErrorBody
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}

public sealed class SoftOnCloudSessionCheckResult
{
    public bool IsValid { get; init; }
    public string Code { get; init; } = "OK";
    public string Message { get; init; } = string.Empty;

    public static SoftOnCloudSessionCheckResult Ok() => new()
    {
        IsValid = true,
        Code = "OK"
    };

    public static SoftOnCloudSessionCheckResult Fail(string code, string message) => new()
    {
        IsValid = false,
        Code = code,
        Message = message
    };
}
