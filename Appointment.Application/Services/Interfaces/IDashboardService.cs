using Appointment.Domain.DTOs.Dashboard.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(
        int orgId,
        DateOnly? asOfDate,
        int? branchId,
        int? professionalId,
        string? period = null,
        CancellationToken cancellationToken = default);
}
