namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Mapped from fn_create_user_session.
/// </summary>
public sealed class UserSessionResponse
{
    public int SessionId { get; set; }
    public DateTimeOffset IssuedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public int OrgId { get; set; }
}
