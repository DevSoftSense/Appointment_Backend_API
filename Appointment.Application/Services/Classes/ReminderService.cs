using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Reminders.Requests;
using Appointment.Domain.DTOs.Reminders.Responses;
using Appointment.Infrastructure.Email;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class ReminderService : IReminderService
{
    private readonly IReminderRepository _reminderRepository;
    private readonly ISmtpEmailSender _emailSender;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReminderService> _logger;

    public ReminderService(
        IReminderRepository reminderRepository,
        ISmtpEmailSender emailSender,
        IConfiguration configuration,
        ILogger<ReminderService> logger)
    {
        _reminderRepository = reminderRepository;
        _emailSender = emailSender;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ReminderListResponse> GetRemindersAsync(
        int orgId, GetRemindersRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetRemindersRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _reminderRepository.GetRemindersAsync(orgId, appId, request, cancellationToken);
        return new ReminderListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ReminderStatsDto> GetStatsAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return await _reminderRepository.GetStatsAsync(orgId, GetAppId(), cancellationToken);
    }

    public async Task<ReminderListItemDto> CreateManualAsync(
        int orgId, long userId, CreateManualReminderRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");
        if (string.IsNullOrWhiteSpace(request.ToAddress) || !request.ToAddress.Contains('@'))
            throw new ArgumentException("A valid email address is required.");
        if (request.ScheduledAt == default)
            throw new ArgumentException("Schedule time is required.");

        return await _reminderRepository.CreateManualAsync(orgId, GetAppId(), userId, request, cancellationToken);
    }

    public async Task SyncForAppointmentAsync(
        int orgId, long appointmentId, long? userId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) return;
        try
        {
            await _reminderRepository.SyncForAppointmentAsync(orgId, GetAppId(), appointmentId, userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not sync reminder for appointment {Id}", appointmentId);
        }
    }

    public async Task CancelForAppointmentAsync(
        int orgId, long appointmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (appointmentId <= 0) return;
        try
        {
            await _reminderRepository.CancelForAppointmentAsync(orgId, GetAppId(), appointmentId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not cancel reminders for appointment {Id}", appointmentId);
        }
    }

    public async Task CancelAsync(int orgId, long notificationId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (notificationId <= 0)
            throw new ArgumentException("Reminder id is required.");
        await _reminderRepository.CancelAsync(orgId, GetAppId(), notificationId, cancellationToken);
    }

    public async Task ProcessDueAsync(CancellationToken cancellationToken = default)
    {
        var appId = GetAppId();
        var due = await _reminderRepository.ClaimDueAsync(appId, orgId: null, batchSize: 25, cancellationToken);
        foreach (var item in due)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(item.ToAddress) || !item.ToAddress.Contains('@'))
                {
                    await _reminderRepository.MarkFailedAsync(
                        appId, item.NotificationId, (int)item.OrgId, "Missing or invalid to_address", cancellationToken);
                    continue;
                }

                var body = item.Message ?? "";
                // Strip leading [Error] blocks from prior failures if reprocessed
                await _emailSender.SendEmailAsync(
                    item.ToAddress,
                    item.Title ?? "Appointment Reminder",
                    body,
                    cancellationToken);

                await _reminderRepository.MarkSentAsync(appId, item.NotificationId, (int)item.OrgId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed sending reminder {Id}", item.NotificationId);
                try
                {
                    await _reminderRepository.MarkFailedAsync(
                        appId, item.NotificationId, (int)item.OrgId, ex.Message, cancellationToken);
                }
                catch (Exception markEx)
                {
                    _logger.LogError(markEx, "Could not mark reminder {Id} failed", item.NotificationId);
                }
            }
        }
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 25;
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }
}
