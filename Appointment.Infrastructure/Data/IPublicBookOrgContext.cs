namespace Appointment.Infrastructure.Data;

/// <summary>
/// Request-scoped org for public QR booking (set after decrypting token <c>t</c>).
/// Used when SoftOnCloud product-connection/service needs orgId and there is no staff JWT.
/// </summary>
public interface IPublicBookOrgContext
{
    int? OrgId { get; set; }
}
