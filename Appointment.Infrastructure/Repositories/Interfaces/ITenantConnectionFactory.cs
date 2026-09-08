namespace Appointment.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Resolves the organisation/product transaction DB connection from SoftOnCloud
/// (GET /api/auth/product-connection). Backend only — never expose to the browser.
/// </summary>
public interface ITenantConnectionFactory
{
    Task<string> GetResolvedConnectionAsync(CancellationToken cancellationToken = default);
}
