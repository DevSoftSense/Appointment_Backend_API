using System.Security.Claims;
using System.Text.Json;
using Appointment.API.Helpers;
using Appointment.Infrastructure.Data;

namespace Appointment.API.Middleware;

/// <summary>
/// Blocks JWTs revoked by Appointment logout / password change.
/// Does NOT call SoftOnCloud session-status (rule: no session-status / session-check).
/// Cross-browser kick is SignalR ForceLogout; this is a backup on the next REST call.
/// </summary>
public sealed class SoftOnCloudSessionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;

    public SoftOnCloudSessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SessionRevocationStore revocationStore)
    {
        // Do not run revoke logic on SignalR negotiate/WebSocket — hub auth is JWT only.
        if (context.Request.Path.StartsWithSegments("/hubs"))
        {
            await _next(context);
            return;
        }

        // Fresh login / password-change re-login must reach session-renew even if a
        // prior logout watermark is still in memory for this user.
        if (context.Request.Path.StartsWithSegments("/api/auth/session-renew"))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated == true
            && IsRevoked(context, revocationStore))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(
                    new
                    {
                        message = "You were signed out on another browser or device. Please sign in again.",
                        code = "SESSION_ENDED"
                    },
                    JsonOptions));
            return;
        }

        await _next(context);
    }

    private static bool IsRevoked(HttpContext context, SessionRevocationStore store)
    {
        var user = context.User;
        var orgRaw = user.FindFirst("org_id")?.Value;
        var userRaw = user.FindFirst("user_id")?.Value;
        if (!int.TryParse(orgRaw, out var orgId) || !int.TryParse(userRaw, out var userId))
            return false;
        return store.IsTokenRevoked(
            orgId,
            userId,
            JwtIatHelper.ReadIssuedAt(context.Request, user));
    }
}
