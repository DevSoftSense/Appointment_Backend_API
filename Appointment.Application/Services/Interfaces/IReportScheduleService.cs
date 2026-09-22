using Appointment.Domain.DTOs.Reports.Requests;
using Appointment.Domain.DTOs.Reports.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IReportScheduleService
{
    Task<ReportScheduleListResponse> ListAsync(
        int orgId, GetReportSchedulesRequest request, CancellationToken cancellationToken = default);

    Task<ReportScheduleItemDto> CreateAsync(
        int orgId, long userId, CreateReportScheduleRequest request, CancellationToken cancellationToken = default);

    Task CancelAsync(int orgId, long notificationId, CancellationToken cancellationToken = default);

    Task ProcessDueAsync(CancellationToken cancellationToken = default);
}
