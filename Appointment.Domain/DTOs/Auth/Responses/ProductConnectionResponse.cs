namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// SoftOnCloud GET /api/auth/product-connection response.
/// resolvedConnectionString is backend-only — never send to the browser.
/// </summary>
public sealed class ProductConnectionResponse
{
    public int ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? DbGroup { get; set; }
    public string? ResolvedConnectionString { get; set; }
    public bool FromCache { get; set; }
}
