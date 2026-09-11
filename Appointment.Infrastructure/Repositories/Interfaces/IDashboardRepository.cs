using Appointment.Domain.DTOs.Dashboard.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IDashboardRepository
{
    Task<DashboardOverviewDto> GetOverviewAsync(
        int orgId,
        int appId,
        DateOnly? asOfDate,
        int? branchId,
        int? professionalId,
        string? period = null,
        CancellationToken cancellationToken = default);
}
