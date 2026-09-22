using Appointment.Domain.DTOs.Reports.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IReportsRepository
{
    Task<ReportOverviewDto> GetOverviewAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportAppointmentsDto> GetAppointmentsAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, string? status,
        int limit, int offset, CancellationToken cancellationToken = default);

    Task<ReportServicesDto> GetServicesAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportProfessionalsDto> GetProfessionalsAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportNoShowDto> GetNoShowAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportCustomersDto> GetCustomersAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId,
        int limit, int offset, CancellationToken cancellationToken = default);
}
