namespace Appointment.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Resolves the organisation/product transaction DB connection from SoftOnCloud.
/// Backend only — never expose connection strings or product service keys to the browser.
/// </summary>
public interface ITenantConnectionFactory
{
    /// <summary>
    /// Staff / logged-in session: GET /api/auth/product-connection with Bearer JWT.
    /// </summary>
    Task<string> GetResolvedConnectionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Public QR / anonymous: GET /api/auth/product-connection/service with X-Product-Service-Key.
    /// </summary>
    Task<string> GetResolvedConnectionForServiceAsync(
        int orgId,
        CancellationToken cancellationToken = default);
}
