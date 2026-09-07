using System.Text.Json;
using Appointment.Domain.DTOs.Appointments.Requests;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_appointment(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class AppointmentRepository : IAppointmentRepository
{
    private const string Fn = "appointment.fn_appointment_appointment";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<AppointmentRepository> _logger;

    public AppointmentRepository(ProductDatabaseHelper db, ILogger<AppointmentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AppointmentListItemDto>> GetAppointmentsAsync(
        int orgId,
        int appId,
        GetAppointmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "list",
                orgId,
                appId,
                search: request.Search,
                status: request.Status,
                dateFrom: request.DateFrom,
                dateTo: request.DateTo,
                customerId: request.CustomerId,
                professionalId: request.ProfessionalId,
                productId: request.ProductId,
                branchId: request.BranchId,
                source: request.Source,
                appointmentType: request.AppointmentType,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<AppointmentListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} list for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Appointment list response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentStatsDto> GetAppointmentStatsAsync(
        int orgId,
        int appId,
        long? customerId = null,
        long? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "stats",
                orgId,
                appId,
                customerId: customerId,
                professionalId: professionalId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new AppointmentStatsDto();

            return JsonSerializer.Deserialize<AppointmentStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new AppointmentStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Appointment stats response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto?> GetAppointmentByIdAsync(
        int orgId,
        int appId,
        long appointmentId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, appointmentId: appointmentId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for appointmentId {AppointmentId}", Fn, appointmentId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for appointmentId {AppointmentId}", Fn, appointmentId);
            throw new InvalidOperationException("Appointment response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto> CreateAppointmentAsync(
        int orgId,
        int appId,
        long createdBy,
        CreateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                customerId: request.CustomerId,
                professionalId: request.ProfessionalId,
                productId: request.ProductId,
                branchId: request.BranchId,
                cabinResourceId: request.CabinResourceId,
                createdBy: createdBy,
                startDatetime: request.StartDatetime,
                endDatetime: request.EndDatetime,
                appointmentDate: request.AppointmentDate,
                status: request.Status,
                appointmentType: request.AppointmentType,
                source: request.Source,
                priority: request.Priority,
                notes: request.Notes,
                paymentStatus: request.PaymentStatus,
                amount: request.Amount,
                isAllDay: request.IsAllDay,
                visibility: request.Visibility,
                reminderMinutesBefore: request.ReminderMinutesBefore,
                internalNote: request.InternalNote,
                fiscalYearId: request.FiscalYearId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Create appointment returned no data.");

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create appointment response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} create for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Create appointment response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto> UpdateAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        UpdateAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                appointmentId: appointmentId,
                updatedBy: updatedBy,
                customerId: request.CustomerId,
                professionalId: request.ProfessionalId,
                productId: request.ProductId,
                branchId: request.BranchId,
                cabinResourceId: request.CabinResourceId,
                startDatetime: request.StartDatetime,
                endDatetime: request.EndDatetime,
                appointmentDate: request.AppointmentDate,
                status: request.Status,
                appointmentType: request.AppointmentType,
                source: request.Source,
                priority: request.Priority,
                notes: request.Notes,
                paymentStatus: request.PaymentStatus,
                amount: request.Amount,
                isAllDay: request.IsAllDay,
                visibility: request.Visibility,
                reminderMinutesBefore: request.ReminderMinutesBefore,
                internalNote: request.InternalNote);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Update appointment returned no data.");

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update appointment response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update for appointmentId {AppointmentId}", Fn, appointmentId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} update for appointmentId {AppointmentId}", Fn, appointmentId);
            throw new InvalidOperationException("Update appointment response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto> CancelAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancelAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "cancel",
                orgId,
                appId,
                appointmentId: appointmentId,
                updatedBy: updatedBy,
                cancellationReason: request.CancellationReason);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Cancel appointment returned no data.");

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Cancel appointment response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} cancel for appointmentId {AppointmentId}", Fn, appointmentId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} cancel for appointmentId {AppointmentId}", Fn, appointmentId);
            throw new InvalidOperationException("Cancel appointment response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto> CheckInAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "check_in",
                orgId,
                appId,
                appointmentId: appointmentId,
                updatedBy: updatedBy);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Check-in returned no data.");

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Check-in response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} check_in for appointmentId {AppointmentId}", Fn, appointmentId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} check_in for appointmentId {AppointmentId}", Fn, appointmentId);
            throw new InvalidOperationException("Check-in response could not be parsed.", ex);
        }
    }

    public async Task<AppointmentDetailDto> CompleteAppointmentAsync(
        int orgId,
        int appId,
        long appointmentId,
        long? updatedBy,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "complete",
                orgId,
                appId,
                appointmentId: appointmentId,
                updatedBy: updatedBy);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Complete returned no data.");

            return JsonSerializer.Deserialize<AppointmentDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Complete response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} complete for appointmentId {AppointmentId}", Fn, appointmentId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} complete for appointmentId {AppointmentId}", Fn, appointmentId);
            throw new InvalidOperationException("Complete response could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? appointmentId = null,
        string? search = null,
        string? status = null,
        DateOnly? dateFrom = null,
        DateOnly? dateTo = null,
        long? customerId = null,
        long? professionalId = null,
        long? productId = null,
        long? branchId = null,
        string? source = null,
        string? appointmentType = null,
        int? limit = null,
        int? offset = null,
        long? createdBy = null,
        long? updatedBy = null,
        DateTimeOffset? startDatetime = null,
        DateTimeOffset? endDatetime = null,
        DateOnly? appointmentDate = null,
        long? cabinResourceId = null,
        string? priority = null,
        string? notes = null,
        string? paymentStatus = null,
        decimal? amount = null,
        bool? isAllDay = null,
        string? visibility = null,
        int? reminderMinutesBefore = null,
        string? cancellationReason = null,
        string? internalNote = null,
        int? fiscalYearId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(appointmentId),
            Varchar(search),
            Varchar(status),
            Date(dateFrom),
            Date(dateTo),
            Bigint(customerId),
            Bigint(professionalId),
            Bigint(productId),
            Bigint(branchId),
            Varchar(source),
            Varchar(appointmentType),
            NullableInt(limit),
            NullableInt(offset),
            Bigint(createdBy),
            Bigint(updatedBy),
            TimestampTz(startDatetime),
            TimestampTz(endDatetime),
            Date(appointmentDate),
            Bigint(cabinResourceId),
            Varchar(priority),
            Text(notes),
            Varchar(paymentStatus),
            Numeric(amount),
            Bool(isAllDay),
            Varchar(visibility),
            NullableInt(reminderMinutesBefore),
            Text(cancellationReason),
            Text(internalNote),
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

    private static NpgsqlParameter TimestampTz(DateTimeOffset? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.TimestampTz
        };

    private static NpgsqlParameter Numeric(decimal? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Numeric
        };

    private static NpgsqlParameter Bool(bool? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Boolean
        };
}
