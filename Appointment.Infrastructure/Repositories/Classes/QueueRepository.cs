using System.Text.Json;
using Appointment.Domain.DTOs.Queue.Requests;
using Appointment.Domain.DTOs.Queue.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_queue(p_action, …). No inline table SQL.
/// </summary>
public sealed class QueueRepository : IQueueRepository
{
    private const string Fn = "appointment.fn_appointment_queue";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<QueueRepository> _logger;

    public QueueRepository(ProductDatabaseHelper db, ILogger<QueueRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<QueueListItemDto>> GetQueueAsync(
        int orgId, int appId, GetQueueRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId,
                search: request.Search,
                status: request.Status,
                queueDate: request.QueueDate,
                customerId: request.CustomerId,
                professionalId: request.ProfessionalId,
                productId: request.ProductId,
                branchId: request.BranchId,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<QueueListItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} list for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Queue list response could not be parsed.", ex);
        }
    }

    public async Task<QueueStatsDto> GetStatsAsync(
        int orgId, int appId, DateOnly? queueDate, long? professionalId, long? branchId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "stats", orgId, appId,
                queueDate: queueDate,
                professionalId: professionalId,
                branchId: branchId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new QueueStatsDto();

            return JsonSerializer.Deserialize<QueueStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new QueueStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Queue stats response could not be parsed.", ex);
        }
    }

    public async Task<QueueListItemDto?> GetByIdAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, queueEntryId: queueEntryId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<QueueListItemDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex) when (ex.MessageText?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
        {
            return null;
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id {Id}", Fn, queueEntryId);
            throw;
        }
    }

    public async Task<IReadOnlyList<PendingCheckInDto>> GetPendingCheckInsAsync(
        int orgId, int appId, DateOnly? queueDate, long? professionalId, long? branchId,
        string? search, int limit, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "pending_checkins", orgId, appId,
                search: search,
                queueDate: queueDate,
                professionalId: professionalId,
                branchId: branchId,
                limit: limit);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<PendingCheckInDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} pending_checkins for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} pending_checkins for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Pending check-ins response could not be parsed.", ex);
        }
    }

    public async Task<QueueListItemDto> CheckInAsync(
        int orgId, int appId, long createdBy, QueueCheckInRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "check_in", orgId, appId,
                appointmentId: request.AppointmentId,
                notes: request.Notes,
                createdBy: createdBy);

            return DeserializeRequired(json, "check_in");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} check_in appointment {Id}", Fn, request.AppointmentId);
            throw;
        }
    }

    public async Task<QueueListItemDto> AddWalkInAsync(
        int orgId, int appId, long createdBy, AddWalkInRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "add_walk_in", orgId, appId,
                queueDate: request.QueueDate,
                customerId: request.CustomerId,
                professionalId: request.ProfessionalId,
                productId: request.ProductId,
                branchId: request.BranchId,
                notes: request.Notes,
                expectedDurationMinutes: request.ExpectedDurationMinutes,
                createdBy: createdBy);

            return DeserializeRequired(json, "add_walk_in");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} add_walk_in for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<QueueListItemDto> CallAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("call", orgId, appId, queueEntryId: queueEntryId);
            return DeserializeRequired(json, "call");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} call {Id}", Fn, queueEntryId);
            throw;
        }
    }

    public async Task<QueueListItemDto> StartAsync(
        int orgId, int appId, long queueEntryId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("start", orgId, appId, queueEntryId: queueEntryId);
            return DeserializeRequired(json, "start");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} start {Id}", Fn, queueEntryId);
            throw;
        }
    }

    public async Task<QueueListItemDto> CompleteAsync(
        int orgId, int appId, long queueEntryId, long? updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "complete", orgId, appId,
                queueEntryId: queueEntryId,
                updatedBy: updatedBy);
            return DeserializeRequired(json, "complete");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} complete {Id}", Fn, queueEntryId);
            throw;
        }
    }

    public async Task<QueueListItemDto> SkipAsync(
        int orgId, int appId, long queueEntryId, string? notes,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "skip", orgId, appId,
                queueEntryId: queueEntryId,
                notes: notes);
            return DeserializeRequired(json, "skip");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} skip {Id}", Fn, queueEntryId);
            throw;
        }
    }

    private static QueueListItemDto DeserializeRequired(string json, string action)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            throw new InvalidOperationException($"Queue {action} returned empty response.");

        return JsonSerializer.Deserialize<QueueListItemDto>(json, PostgresJsonOptions.Options)
               ?? throw new InvalidOperationException($"Queue {action} response could not be parsed.");
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? queueEntryId = null,
        long? appointmentId = null,
        string? search = null,
        string? status = null,
        DateOnly? queueDate = null,
        long? customerId = null,
        long? professionalId = null,
        long? productId = null,
        long? branchId = null,
        string? notes = null,
        int? expectedDurationMinutes = null,
        long? createdBy = null,
        long? updatedBy = null,
        int? limit = null,
        int? offset = null,
        int? fiscalYearId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(queueEntryId),
            Bigint(appointmentId),
            Varchar(search),
            Varchar(status),
            Date(queueDate),
            Bigint(customerId),
            Bigint(professionalId),
            Bigint(productId),
            Bigint(branchId),
            Text(notes),
            NullableInt(expectedDurationMinutes),
            Bigint(createdBy),
            Bigint(updatedBy),
            NullableInt(limit),
            NullableInt(offset),
            NullableInt(fiscalYearId));

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Bigint(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Text(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Text
        };

    private static NpgsqlParameter Date(DateOnly? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
