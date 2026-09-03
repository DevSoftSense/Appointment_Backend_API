namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Final response returned to the client after a successful login.
/// Token = short-lived JWT access token.
/// RefreshToken = long-lived opaque token (only the hash is stored in the DB).
/// User = full org context from fn_get_org_login_context.
/// </summary>
public sealed class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public int SessionId { get; set; }
    public bool RememberMe { get; set; }
    public OrgLoginContextResponse User { get; set; } = new();
}
