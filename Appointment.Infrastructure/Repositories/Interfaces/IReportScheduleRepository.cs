using Appointment.Domain.DTOs.Reports.Requests;
using Appointment.Domain.DTOs.Reports.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IReportScheduleRepository
{
    Task<IReadOnlyList<ReportScheduleItemDto>> ListAsync(
        int orgId, int appId, GetReportSchedulesRequest request, CancellationToken cancellationToken = default);

    Task<ReportScheduleItemDto> CreateAsync(
        int orgId, int appId, long userId, string title, string definitionJson,
        string toAddress, DateTimeOffset scheduledAt, CancellationToken cancellationToken = default);

    Task CancelAsync(int orgId, int appId, long notificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReportScheduleDueItemDto>> ClaimDueAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default);

    Task MarkSentAsync(int appId, long notificationId, int? orgId, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int appId, long notificationId, int? orgId, string? errorMessage, CancellationToken cancellationToken = default);

    Task RescheduleNextAsync(
        int appId, long notificationId, int? orgId, CancellationToken cancellationToken = default);
}
