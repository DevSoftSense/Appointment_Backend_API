namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Mapped from fn_record_login_attempt.
/// session_authorized is the authoritative gate — not just the password check result.
/// </summary>
public sealed class LoginAttemptResponse
{
    public bool IsAccountLocked { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public int FailedLoginAttempts { get; set; }
    public bool SessionAuthorized { get; set; }
}
