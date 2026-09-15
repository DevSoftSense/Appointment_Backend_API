using System.Text.Json;
using Appointment.Domain.DTOs.Reminders.Requests;
using Appointment.Domain.DTOs.Reminders.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class ReminderRepository : IReminderRepository
{
    private const string Fn = "appointment.fn_appointment_reminder";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ReminderRepository> _logger;

    public ReminderRepository(ProductDatabaseHelper db, ILogger<ReminderRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReminderListItemDto>> GetRemindersAsync(
        int orgId, int appId, GetRemindersRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                sendStatus: request.SendStatus,
                referenceEntity: request.ReferenceEntity,
                appointmentId: request.AppointmentId,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<ReminderListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<ReminderStatsDto> GetStatsAsync(int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync("stats", orgId, appId);
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return new ReminderStatsDto();
        return JsonSerializer.Deserialize<ReminderStatsDto>(json, PostgresJsonOptions.Options) ?? new ReminderStatsDto();
    }

    public async Task<ReminderListItemDto> CreateManualAsync(
        int orgId, int appId, long userId, CreateManualReminderRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync(
            "create_manual", orgId, appId,
            userId: userId,
            appointmentId: request.AppointmentId,
            title: request.Title,
            message: request.Message,
            toAddress: request.ToAddress,
            scheduledAt: request.ScheduledAt.UtcDateTime);

        var item = JsonSerializer.Deserialize<ReminderListItemDto>(json, PostgresJsonOptions.Options);
        return item ?? throw new InvalidOperationException("Failed to create reminder.");
    }

    public async Task SyncForAppointmentAsync(
        int orgId, int appId, long appointmentId, long? userId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            await CallAsync("sync_for_appointment", orgId, appId, appointmentId: appointmentId, userId: userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reminder sync failed for appointment {AppointmentId}", appointmentId);
            throw;
        }
    }

    public async Task CancelForAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("cancel_for_appointment", orgId, appId, appointmentId: appointmentId);
    }

    public async Task CancelAsync(int orgId, int appId, long notificationId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("cancel", orgId, appId, notificationId: notificationId);
    }

    public async Task<IReadOnlyList<ReminderDueItemDto>> ClaimDueAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync(
            "claim_due",
            orgId,
            appId,
            batchSize: batchSize);

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return [];

        return JsonSerializer.Deserialize<List<ReminderDueItemDto>>(json, PostgresJsonOptions.Options) ?? [];
    }

    public async Task MarkSentAsync(int appId, long notificationId, int? orgId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("mark_sent", orgId, appId, notificationId: notificationId);
    }

    public async Task MarkFailedAsync(
        int appId, long notificationId, int? orgId, string? errorMessage, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("mark_failed", orgId, appId, notificationId: notificationId, errorMessage: errorMessage);
    }

    private Task<string> CallAsync(
        string action,
        int? orgId,
        int appId,
        long? notificationId = null,
        long? appointmentId = null,
        long? userId = null,
        string? search = null,
        string? sendStatus = null,
        string? referenceEntity = null,
        int? limit = null,
        int? offset = null,
        string? title = null,
        string? message = null,
        string? toAddress = null,
        DateTime? scheduledAt = null,
        string? errorMessage = null,
        int? batchSize = null)
    {
        return _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Big(notificationId),
            Big(appointmentId),
            Big(userId),
            Varchar(search),
            Varchar(sendStatus),
            Varchar(referenceEntity),
            Int(limit),
            Int(offset),
            Varchar(title),
            Text(message),
            Varchar(toAddress),
            Ts(scheduledAt),
            Text(errorMessage),
            Int(batchSize));
    }

    private static NpgsqlParameter Varchar(string? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value };

    private static NpgsqlParameter Text(string? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Text, Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value };

    private static NpgsqlParameter Int(int? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Integer, Value = value.HasValue ? value.Value : DBNull.Value };

    private static NpgsqlParameter Big(long? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Bigint, Value = value.HasValue ? value.Value : DBNull.Value };

    private static NpgsqlParameter Ts(DateTime? value) =>
        new()
        {
            NpgsqlDbType = NpgsqlDbType.TimestampTz,
            Value = value.HasValue
                ? (value.Value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
                    : value.Value.ToUniversalTime())
                : DBNull.Value
        };
}
