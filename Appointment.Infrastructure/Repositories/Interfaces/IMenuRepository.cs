using Appointment.Domain.DTOs.Menu.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IMenuRepository
{
    Task<IReadOnlyList<MenuItemDto>> ListSidebarAsync(int appId, CancellationToken cancellationToken = default);
}
