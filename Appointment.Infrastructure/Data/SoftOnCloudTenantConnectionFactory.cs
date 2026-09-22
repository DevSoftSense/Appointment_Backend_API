using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Appointment.Domain.DTOs.Auth.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// SoftOnCloud product-connection:
/// <list type="bullet">
/// <item>JWT path — logged-in staff (Bearer token on current request)</item>
/// <item>Service path — public QR / anonymous (X-Product-Service-Key + orgId)</item>
/// </list>
/// Never log full connection strings or the product service key.
/// </summary>
public sealed class SoftOnCloudTenantConnectionFactory : ITenantConnectionFactory
{
    private const string ServiceCacheKeyPrefix = "soc:product-conn:service:org:";
    private static readonly TimeSpan ServiceCacheTtl = TimeSpan.FromMinutes(10);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SoftOnCloudTenantConnectionFactory> _logger;
    private readonly AsyncLocal<string?> _requestCache = new();

    public SoftOnCloudTenantConnectionFactory(
        IHttpContextAccessor httpContextAccessor,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        ILogger<SoftOnCloudTenantConnectionFactory> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<string> GetResolvedConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_requestCache.Value))
        {
            return _requestCache.Value!;
        }

        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader)
            || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            throw new UnauthorizedAccessException("Missing SoftOnCloud bearer token.");
        }

        var productId = ResolveProductId();
        var client = _httpClientFactory.CreateClient("SoftOnCloud");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/auth/product-connection?productId={productId}");
        request.Headers.TryAddWithoutValidation("Authorization", authHeader);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        var connectionString = await ReadConnectionStringAsync(
            response,
            "product-connection",
            productId,
            orgId: null,
            cancellationToken);

        _requestCache.Value = connectionString;
        return connectionString;
    }

    public async Task<string> GetResolvedConnectionForServiceAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        if (orgId <= 0)
            throw new ArgumentOutOfRangeException(nameof(orgId), "orgId must be a positive SoftOnCloud organisation id.");

        var cacheKey = ServiceCacheKeyPrefix + orgId;
        if (_memoryCache.TryGetValue(cacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
        {
            return cached;
        }

        var serviceKey = _configuration["SoftOnCloud:ProductServiceKey"];
        if (string.IsNullOrWhiteSpace(serviceKey))
        {
            throw new InvalidOperationException(
                "SoftOnCloud:ProductServiceKey is not configured. " +
                "Set the Appointment product service key from SoftOnCloud platform on the API server " +
                "(SoftOnCloud:ProductServiceKey or env SoftOnCloud__ProductServiceKey). " +
                "Required for public QR booking when UseProductConnectionDb=true.");
        }

        var productId = ResolveProductId();
        var client = _httpClientFactory.CreateClient("SoftOnCloud");

        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/auth/product-connection/service?orgId={orgId}&productId={productId}");
        request.Headers.TryAddWithoutValidation("X-Product-Service-Key", serviceKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await client.SendAsync(request, cancellationToken);
        var connectionString = await ReadConnectionStringAsync(
            response,
            "product-connection/service",
            productId,
            orgId,
            cancellationToken);

        _memoryCache.Set(
            cacheKey,
            connectionString,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ServiceCacheTtl
            });

        return connectionString;
    }

    private int ResolveProductId()
    {
        return _configuration.GetValue<int?>("SoftOnCloud:ProductId")
               ?? _configuration.GetValue<int?>("Appointment:ProductId")
               ?? _configuration.GetValue<int?>("Appointment:AppId")
               ?? throw new InvalidOperationException(
                   "SoftOnCloud:ProductId (or Appointment:ProductId) is not configured.");
    }

    private async Task<string> ReadConnectionStringAsync(
        HttpResponseMessage response,
        string endpointLabel,
        int productId,
        int? orgId,
        CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (endpointLabel.Contains("service", StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException(
                    "Invalid or missing SoftOnCloud product service key.");
            }

            throw new UnauthorizedAccessException("SoftOnCloud session ended.");
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            throw new InvalidOperationException(
                orgId.HasValue
                    ? $"Product connection unavailable for org {orgId.Value} (product {productId})."
                    : "Product connection unavailable for this user.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "SoftOnCloud {Endpoint} failed. Status={StatusCode} ProductId={ProductId} OrgId={OrgId} BodyLength={Length}",
                endpointLabel,
                (int)response.StatusCode,
                productId,
                orgId,
                body?.Length ?? 0);
            throw new InvalidOperationException(
                $"SoftOnCloud {endpointLabel} failed with status {(int)response.StatusCode}.");
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ProductConnectionResponse>(
            stream,
            JsonOptions,
            cancellationToken);

        var connectionString = payload?.ResolvedConnectionString;
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"Empty product connection from SoftOnCloud ({endpointLabel}).");
        }

        _logger.LogInformation(
            "SoftOnCloud {Endpoint} resolved DB for ProductId={ProductId} OrgId={OrgId} FromCache={FromCache}",
            endpointLabel,
            productId,
            orgId,
            payload?.FromCache ?? false);

        return connectionString;
    }
}
