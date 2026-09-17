namespace Appointment.Application.Services.Interfaces;

/// <summary>
/// Opaque encrypted tokens for public QR booking links (hides raw org id in the URL).
/// </summary>
public interface IPublicBookTokenService
{
    /// <summary>Create a URL-safe token for this organisation (Appointment app).</summary>
    string CreateToken(int orgId);

    /// <summary>Decrypt and validate token; returns false if forged / corrupt / wrong app.</summary>
    bool TryResolve(string? token, out int orgId);
}
