using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Dashboard.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IConfiguration _configuration;

    public DashboardService(IDashboardRepository dashboardRepository, IConfiguration configuration)
    {
        _dashboardRepository = dashboardRepository;
        _configuration = configuration;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(
        int orgId,
        DateOnly? asOfDate,
        int? branchId,
        int? professionalId,
        string? period = null,
        CancellationToken cancellationToken = default)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");

        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");

        return await _dashboardRepository.GetOverviewAsync(
            orgId, appId, asOfDate, branchId, professionalId, period, cancellationToken);
    }
}
