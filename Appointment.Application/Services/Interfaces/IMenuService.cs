using Appointment.Domain.DTOs.Menu.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyList<MenuItemDto>> GetSidebarAsync(
        int orgId,
        IReadOnlyList<string>? roleCodes = null,
        string? userType = null,
        CancellationToken cancellationToken = default);
}
