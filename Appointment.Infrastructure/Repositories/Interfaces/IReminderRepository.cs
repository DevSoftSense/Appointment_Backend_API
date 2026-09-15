using Appointment.Domain.DTOs.Reminders.Requests;
using Appointment.Domain.DTOs.Reminders.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IReminderRepository
{
    Task<IReadOnlyList<ReminderListItemDto>> GetRemindersAsync(
        int orgId, int appId, GetRemindersRequest request, CancellationToken cancellationToken = default);

    Task<ReminderStatsDto> GetStatsAsync(int orgId, int appId, CancellationToken cancellationToken = default);

    Task<ReminderListItemDto> CreateManualAsync(
        int orgId, int appId, long userId, CreateManualReminderRequest request, CancellationToken cancellationToken = default);

    Task SyncForAppointmentAsync(
        int orgId, int appId, long appointmentId, long? userId, CancellationToken cancellationToken = default);

    Task CancelForAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default);

    Task CancelAsync(int orgId, int appId, long notificationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ReminderDueItemDto>> ClaimDueAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default);

    Task MarkSentAsync(int appId, long notificationId, int? orgId, CancellationToken cancellationToken = default);

    Task MarkFailedAsync(
        int appId, long notificationId, int? orgId, string? errorMessage, CancellationToken cancellationToken = default);
}
