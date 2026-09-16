using Appointment.Domain.DTOs.Reports.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IReportsService
{
    Task<ReportOverviewDto> GetOverviewAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportAppointmentsDto> GetAppointmentsAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, string? status,
        int limit, int offset, CancellationToken cancellationToken = default);

    Task<ReportServicesDto> GetServicesAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportProfessionalsDto> GetProfessionalsAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);

    Task<ReportNoShowDto> GetNoShowAsync(
        int orgId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default);
}
