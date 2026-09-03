namespace Appointment.Domain.DTOs.Auth.Requests;

public sealed class LoginRequest
{
    public int OrgId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
}
