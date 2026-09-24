using Appointment.Domain.DTOs.Menu.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IMenuService
{
    Task<IReadOnlyList<MenuItemDto>> GetSidebarAsync(CancellationToken cancellationToken = default);
}
