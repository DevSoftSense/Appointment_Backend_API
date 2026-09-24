using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Menu.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class MenuService : IMenuService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IConfiguration _configuration;

    public MenuService(IMenuRepository menuRepository, IConfiguration configuration)
    {
        _menuRepository = menuRepository;
        _configuration = configuration;
    }

    public async Task<IReadOnlyList<MenuItemDto>> GetSidebarAsync(
        CancellationToken cancellationToken = default)
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");

        return await _menuRepository.ListSidebarAsync(appId, cancellationToken);
    }
}
