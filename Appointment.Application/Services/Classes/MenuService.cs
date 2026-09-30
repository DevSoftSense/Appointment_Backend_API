using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Menu.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;

    public MenuService(
        IMenuRepository menuRepository,
        IPermissionService permissionService,
        IConfiguration configuration)
    {
        _menuRepository = menuRepository;
        _permissionService = permissionService;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetSidebarAsync(
        int orgId,
        IReadOnlyList<string>? roleCodes = null,
        CancellationToken cancellationToken = default)
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");

        var all = await _menuRepository.ListSidebarAsync(appId, cancellationToken);
        if (orgId <= 0)
            return all;

        var my = await _permissionService.GetMyAsync(
            orgId, roleCodes ?? [], false, cancellationToken);

        // Fail-open / admin → show all
        if (my.IsAdmin || my.FailOpen)
            return all;

        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var m in my.Modules ?? [])
        {
            if (!m.CanView) continue;
            var c = (m.MenuCode ?? "").Trim().ToUpperInvariant();
            if (c.Length == 0) continue;
            allowed.Add(c);
            if (c.StartsWith("APPOINTMENT_", StringComparison.Ordinal))
                allowed.Add(c["APPOINTMENT_".Length..]);
            else
                allowed.Add("APPOINTMENT_" + c);
        }

        if (allowed.Count == 0)
            return [];

        static bool IsAllowed(HashSet<string> set, string? menuCode)
        {
            var c = (menuCode ?? "").Trim().ToUpperInvariant();
            return c.Length > 0 && set.Contains(c);
        }

        return all
            .Where(m => IsAllowed(allowed, m.MenuCode))
            .Select(m =>
            {
                // Filter Settings children by permission too
                if (m.Children is { Count: > 0 })
                {
                    m.Children = m.Children
                        .Where(c => IsAllowed(allowed, c.MenuCode))
                        .ToList();
                }
                return m;
            })
            .ToList();
    }
}
