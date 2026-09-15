using Appointment.Domain.DTOs.Reminders.Requests;
using Appointment.Domain.DTOs.Reminders.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IReminderService
{
    Task<ReminderListResponse> GetRemindersAsync(int orgId, GetRemindersRequest request, CancellationToken cancellationToken = default);
    Task<ReminderStatsDto> GetStatsAsync(int orgId, CancellationToken cancellationToken = default);
    Task<ReminderListItemDto> CreateManualAsync(int orgId, long userId, CreateManualReminderRequest request, CancellationToken cancellationToken = default);
    Task SyncForAppointmentAsync(int orgId, long appointmentId, long? userId, CancellationToken cancellationToken = default);
    Task CancelForAppointmentAsync(int orgId, long appointmentId, CancellationToken cancellationToken = default);
    Task CancelAsync(int orgId, long notificationId, CancellationToken cancellationToken = default);
    Task ProcessDueAsync(CancellationToken cancellationToken = default);
}
