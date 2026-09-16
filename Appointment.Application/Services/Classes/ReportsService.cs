using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Reports.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class ReportsService : IReportsService
{
    private readonly IReportsRepository _reportsRepository;
    private readonly IConfiguration _configuration;

    public ReportsService(IReportsRepository reportsRepository, IConfiguration configuration)
    {
        _reportsRepository = reportsRepository;
        _configuration = configuration;
    }

    public Task<ReportOverviewDto> GetOverviewAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default)
    {
        Validate(orgId, fromDate, toDate);
        var appId = GetAppId();
        return _reportsRepository.GetOverviewAsync(
            orgId, appId, fromDate, toDate, branchId, professionalId, cancellationToken);
    }

    public Task<ReportAppointmentsDto> GetAppointmentsAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, string? status,
        int limit, int offset, CancellationToken cancellationToken = default)
    {
        Validate(orgId, fromDate, toDate);
        if (limit <= 0) limit = 100;
        if (limit > 500) limit = 500;
        if (offset < 0) offset = 0;
        var appId = GetAppId();
        return _reportsRepository.GetAppointmentsAsync(
            orgId, appId, fromDate, toDate, branchId, professionalId, status, limit, offset, cancellationToken);
    }

    public Task<ReportServicesDto> GetServicesAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default)
    {
        Validate(orgId, fromDate, toDate);
        var appId = GetAppId();
        return _reportsRepository.GetServicesAsync(
            orgId, appId, fromDate, toDate, branchId, professionalId, cancellationToken);
    }

    public Task<ReportProfessionalsDto> GetProfessionalsAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default)
    {
        Validate(orgId, fromDate, toDate);
        var appId = GetAppId();
        return _reportsRepository.GetProfessionalsAsync(
            orgId, appId, fromDate, toDate, branchId, professionalId, cancellationToken);
    }

    public Task<ReportNoShowDto> GetNoShowAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default)
    {
        Validate(orgId, fromDate, toDate);
        var appId = GetAppId();
        return _reportsRepository.GetNoShowAsync(
            orgId, appId, fromDate, toDate, branchId, professionalId, cancellationToken);
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId;
    }

    private static void Validate(int orgId, DateOnly fromDate, DateOnly toDate)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
        if (toDate < fromDate)
            throw new ArgumentException("toDate must be on or after fromDate.");
        if (toDate.DayNumber - fromDate.DayNumber > 366)
            throw new ArgumentException("Date range cannot exceed 366 days.");
    }
}
