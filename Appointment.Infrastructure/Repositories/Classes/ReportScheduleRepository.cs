using System.Text.Json;
using Appointment.Domain.DTOs.Reports.Requests;
using Appointment.Domain.DTOs.Reports.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class ReportScheduleRepository : IReportScheduleRepository
{
    private const string Fn = "appointment.fn_appointment_report_schedule";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ReportScheduleRepository> _logger;

    public ReportScheduleRepository(ProductDatabaseHelper db, ILogger<ReportScheduleRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReportScheduleItemDto>> ListAsync(
        int orgId, int appId, GetReportSchedulesRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync(
            "list", orgId, appId,
            sendStatus: request.SendStatus,
            limit: request.Limit,
            offset: request.Offset);

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return [];

        return JsonSerializer.Deserialize<List<ReportScheduleItemDto>>(json, PostgresJsonOptions.Options) ?? [];
    }

    public async Task<ReportScheduleItemDto> CreateAsync(
        int orgId, int appId, long userId, string title, string definitionJson,
        string toAddress, DateTimeOffset scheduledAt, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync(
            "create", orgId, appId,
            userId: userId,
            title: title,
            message: definitionJson,
            toAddress: toAddress,
            scheduledAt: scheduledAt.UtcDateTime);

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            throw new InvalidOperationException("Create schedule returned empty response.");

        return JsonSerializer.Deserialize<ReportScheduleItemDto>(json, PostgresJsonOptions.Options)
               ?? throw new InvalidOperationException("Could not parse create schedule response.");
    }

    public async Task CancelAsync(int orgId, int appId, long notificationId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("cancel", orgId, appId, notificationId: notificationId);
    }

    public async Task<IReadOnlyList<ReportScheduleDueItemDto>> ClaimDueAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync("claim_due", orgId, appId, batchSize: batchSize);
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return [];
        return JsonSerializer.Deserialize<List<ReportScheduleDueItemDto>>(json, PostgresJsonOptions.Options) ?? [];
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

    public async Task RescheduleNextAsync(
        int appId, long notificationId, int? orgId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("reschedule_next", orgId, appId, notificationId: notificationId);
    }

    private Task<string> CallAsync(
        string action,
        int? orgId,
        int appId,
        long? notificationId = null,
        long? userId = null,
        string? sendStatus = null,
        int? limit = null,
        int? offset = null,
        string? title = null,
        string? message = null,
        string? toAddress = null,
        DateTime? scheduledAt = null,
        string? errorMessage = null,
        int? batchSize = null)
    {
        try
        {
            return _db.ExecuteJsonFunctionAsync(
                Fn,
                Varchar(action),
                Int(orgId),
                Int(appId),
                Big(notificationId),
                Big(userId),
                Varchar(sendStatus),
                Int(limit),
                Int(offset),
                Varchar(title),
                Text(message),
                Varchar(toAddress),
                Ts(scheduledAt),
                Text(errorMessage),
                Int(batchSize));
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} {Action}", Fn, action);
            throw;
        }
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
