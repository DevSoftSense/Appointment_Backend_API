using Appointment.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Appointment.API.Services;

public interface ISessionForceLogoutNotifier
{
    Task NotifyUserAsync(
        int orgId,
        int userId,
        string reason,
        string? exceptConnectionId = null,
        CancellationToken cancellationToken = default);
}

public sealed class SessionForceLogoutNotifier : ISessionForceLogoutNotifier
{
    private readonly IHubContext<SessionHub> _hub;

    public SessionForceLogoutNotifier(IHubContext<SessionHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyUserAsync(
        int orgId,
        int userId,
        string reason,
        string? exceptConnectionId = null,
        CancellationToken cancellationToken = default)
    {
        if (orgId <= 0 || userId <= 0) return;

        var group = SessionHub.UserGroup(orgId, userId);
        var payload = new
        {
            reason = string.IsNullOrWhiteSpace(reason) ? "session_ended" : reason,
            at = DateTimeOffset.UtcNow
        };

        // Password change: never kick the browser that is silently re-logging in.
        if (!string.IsNullOrWhiteSpace(exceptConnectionId))
        {
            await _hub.Clients
                .GroupExcept(group, new[] { exceptConnectionId.Trim() })
                .SendAsync("ForceLogout", payload, cancellationToken);
            return;
        }

        await _hub.Clients.Group(group).SendAsync("ForceLogout", payload, cancellationToken);
    }
}
