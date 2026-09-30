using System.Text.Json;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Domain.DTOs.FollowUps.Requests;
using Appointment.Domain.DTOs.FollowUps.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_follow_up(p_action, …). No inline table SQL.
/// </summary>
public sealed class FollowUpRepository : IFollowUpRepository
{
    private const string Fn = "appointment.fn_appointment_follow_up";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<FollowUpRepository> _logger;

    public FollowUpRepository(ProductDatabaseHelper db, ILogger<FollowUpRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<FollowUpListItemDto>> ListByParentAsync(
        int orgId, int appId, long parentAppointmentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("list_by_parent", orgId, appId, parentAppointmentId);
            if (string.IsNullOrWhiteSpace(json) || json == "null" || json == "[]")
                return Array.Empty<FollowUpListItemDto>();

            return JsonSerializer.Deserialize<List<FollowUpListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? new List<FollowUpListItemDto>();
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} list_by_parent", Fn);
            throw;
        }
    }

    public async Task<AppointmentDetailDto> CreateOneAsync(
        int orgId, int appId, long parentAppointmentId, long createdBy,
        CreateFollowUpRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create_one", orgId, appId, parentAppointmentId,
                createdBy: createdBy,
                payload: BuildOnePayload(request));
            return DeserializeDetail(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create_one", Fn);
            throw;
        }
    }

    public async Task<FollowUpSeriesPreviewDto> PreviewSeriesAsync(
        int orgId, int appId, long parentAppointmentId,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "preview_series", orgId, appId, parentAppointmentId,
                payload: BuildSeriesPayload(request));
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new FollowUpSeriesPreviewDto { ParentAppointmentId = parentAppointmentId };
            return JsonSerializer.Deserialize<FollowUpSeriesPreviewDto>(json, PostgresJsonOptions.Options)
                   ?? new FollowUpSeriesPreviewDto { ParentAppointmentId = parentAppointmentId };
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} preview_series", Fn);
            throw;
        }
    }

    public async Task<FollowUpSeriesResultDto> CreateSeriesAsync(
        int orgId, int appId, long parentAppointmentId, long createdBy,
        CreateFollowUpSeriesRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create_series", orgId, appId, parentAppointmentId,
                createdBy: createdBy,
                payload: BuildSeriesPayload(request));
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException($"{Fn} create_series returned no data");
            return JsonSerializer.Deserialize<FollowUpSeriesResultDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} create_series returned empty payload");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create_series", Fn);
            throw;
        }
    }

    private static string BuildOnePayload(CreateFollowUpRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["start_datetime"] = request.StartDatetime.ToString("o"),
            ["notes"] = request.Notes,
            ["product_id"] = request.ProductId,
            ["professional_id"] = request.ProfessionalId,
            ["cabin_resource_id"] = request.CabinResourceId,
            ["reminder_minutes_before"] = request.ReminderMinutesBefore
        };
        return JsonSerializer.Serialize(payload);
    }

    private static string BuildSeriesPayload(CreateFollowUpSeriesRequest request)
    {
        var payload = new Dictionary<string, object?>
        {
            ["mode"] = request.Mode,
            ["interval_unit"] = request.IntervalUnit,
            ["interval_value"] = request.IntervalValue,
            ["count"] = request.Count,
            ["ends_on"] = request.EndsOn?.ToString("yyyy-MM-dd"),
            ["first_start_datetime"] = request.FirstStartDatetime.ToString("o"),
            ["notes"] = request.Notes,
            ["product_id"] = request.ProductId,
            ["professional_id"] = request.ProfessionalId,
            ["cabin_resource_id"] = request.CabinResourceId,
            ["reminder_minutes_before"] = request.ReminderMinutesBefore
        };
        return JsonSerializer.Serialize(payload);
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? appointmentId = null,
        long? createdBy = null,
        string? payload = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(appointmentId),
            Bigint(createdBy),
            Jsonb(payload));

    private static AppointmentDetailDto DeserializeDetail(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            throw new InvalidOperationException($"{Fn} returned no data");
        return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
               ?? throw new InvalidOperationException($"{Fn} returned empty appointment");
    }

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Bigint(long? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Bigint };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Jsonb(string? json) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(json) ? DBNull.Value : json,
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };
}
