using Appointment.API.Helpers;
using Appointment.API.Services;
using Appointment.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Appointment.API.Controllers;

/// <summary>
/// One-shot logout revoke + SignalR push to other browsers (no session-check poll).
/// </summary>
[Authorize]
[ApiController]
[Route("api/auth")]
public sealed class AuthSessionController : ControllerBase
{
    private readonly SessionRevocationStore _revocationStore;
    private readonly ISessionForceLogoutNotifier _forceLogoutNotifier;

    public AuthSessionController(
        SessionRevocationStore revocationStore,
        ISessionForceLogoutNotifier forceLogoutNotifier)
    {
        _revocationStore = revocationStore;
        _forceLogoutNotifier = forceLogoutNotifier;
    }

    public sealed class LogoutRequest
    {
        /// <summary>SignalR connection id to keep (password change on this browser).</summary>
        public string? ExceptConnectionId { get; set; }

        /// <summary>session_ended | password_changed</summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// POST api/auth/logout — revoke older JWTs and push ForceLogout to other devices.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest? body,
        CancellationToken cancellationToken)
    {
        var orgRaw = User.FindFirst("org_id")?.Value;
        var userRaw = User.FindFirst("user_id")?.Value;
        if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId)
            || orgId <= 0 || userId <= 0)
        {
            return Ok(new { ok = true, revoked = false });
        }

        _revocationStore.RevokeUser(orgId, userId, DateTimeOffset.UtcNow);

        var reason = string.Equals(body?.Reason, "password_changed", StringComparison.OrdinalIgnoreCase)
            ? "password_changed"
            : "session_ended";

        await _forceLogoutNotifier.NotifyUserAsync(
            orgId,
            userId,
            reason,
            body?.ExceptConnectionId,
            cancellationToken);

        return Ok(new { ok = true, revoked = true });
    }

    /// <summary>
    /// POST api/auth/session-renew — after login or password-change silent re-login.
    /// Aligns revoke watermark to THIS JWT's iat (from Bearer payload, not claims).
    /// Older tokens stay blocked; this token is allowed. Never uses UtcNow fallback
    /// (that could revoke a token issued earlier in the same second).
    /// </summary>
    [HttpPost("session-renew")]
    public IActionResult SessionRenew()
    {
        var orgRaw = User.FindFirst("org_id")?.Value;
        var userRaw = User.FindFirst("user_id")?.Value;
        if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId)
            || orgId <= 0 || userId <= 0)
        {
            return Ok(new { ok = true, renewed = false });
        }

        var iatUnix = JwtIatHelper.ReadIssuedAtUnix(Request, User);
        if (iatUnix is null || iatUnix <= 0)
        {
            // Cannot read iat — clear revoke so a fresh SoftOnCloud login is never stuck.
            _revocationStore.ClearRevocation(orgId, userId);
            return Ok(new { ok = true, renewed = true, cleared = true });
        }

        _revocationStore.SetRevokeWatermark(orgId, userId, iatUnix.Value);
        return Ok(new { ok = true, renewed = true });
    }
}
