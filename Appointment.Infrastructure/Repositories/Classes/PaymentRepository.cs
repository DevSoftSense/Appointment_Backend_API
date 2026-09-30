using System.Text.Json;
using Appointment.Domain.DTOs.Payments.Requests;
using Appointment.Domain.DTOs.Payments.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_payment(p_action, …). No inline table SQL.
/// </summary>
public sealed class PaymentRepository : IPaymentRepository
{
    private const string Fn = "appointment.fn_appointment_payment";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<PaymentRepository> _logger;

    public PaymentRepository(ProductDatabaseHelper db, ILogger<PaymentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaymentEntryListResponse> ListAsync(
        int orgId, int appId, GetPaymentEntriesRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list", orgId, appId, appointmentId: null,
                payload: BuildListPayload(request));
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new PaymentEntryListResponse();
            return JsonSerializer.Deserialize<PaymentEntryListResponse>(json, PostgresJsonOptions.Options)
                   ?? new PaymentEntryListResponse();
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<PaymentEntryStatsDto> ListStatsAsync(
        int orgId, int appId, GetPaymentEntriesRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list_stats", orgId, appId, appointmentId: null,
                payload: BuildListPayload(request, statsOnly: true));
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new PaymentEntryStatsDto();
            return JsonSerializer.Deserialize<PaymentEntryStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new PaymentEntryStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} list_stats", Fn);
            throw;
        }
    }

    public async Task<AppointmentPaymentDto> GetByAppointmentAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_appointment", orgId, appId, appointmentId);
            return DeserializeRequired(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} get_by_appointment", Fn);
            throw;
        }
    }

    public async Task<AppointmentPaymentDto> CreateAsync(
        int orgId, int appId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create", orgId, appId, appointmentId,
                payload: BuildBillPayload(request),
                createdBy: userId,
                fiscalYearId: request.FiscalYearId);
            return DeserializeRequired(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<AppointmentPaymentDto> UpdateBillAsync(
        int orgId, int appId, long appointmentId, long userId, RecordPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update_bill", orgId, appId, appointmentId,
                payload: BuildBillPayload(request),
                createdBy: userId,
                fiscalYearId: request.FiscalYearId);
            return DeserializeRequired(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} update_bill", Fn);
            throw;
        }
    }

    public async Task<AppointmentPaymentDto> AddPaymentAsync(
        int orgId, int appId, long appointmentId, long userId, AddPaymentReceiptRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var payload = new Dictionary<string, object?>
            {
                ["amount_received"] = request.AmountReceived,
                ["bill_date"] = request.BillDate?.ToString("yyyy-MM-dd"),
                ["payment_method"] = request.PaymentMethod,
                ["payment_note"] = request.PaymentNote
            };
            var json = await CallAsync(
                "add_payment", orgId, appId, appointmentId,
                payload: JsonSerializer.Serialize(payload),
                createdBy: userId,
                fiscalYearId: request.FiscalYearId);
            return DeserializeRequired(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} add_payment", Fn);
            throw;
        }
    }

    public async Task<AppointmentPaymentDto> VoidAsync(
        int orgId, int appId, long appointmentId, long userId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("void", orgId, appId, appointmentId, createdBy: userId);
            return DeserializeRequired(json);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} void", Fn);
            throw;
        }
    }

    private static string BuildListPayload(GetPaymentEntriesRequest request, bool statsOnly = false)
    {
        var payload = new Dictionary<string, object?>
        {
            ["from_date"] = request.FromDate?.ToString("yyyy-MM-dd"),
            ["to_date"] = request.ToDate?.ToString("yyyy-MM-dd"),
            ["branch_id"] = request.BranchId
        };
        if (!statsOnly)
        {
            payload["payment_status"] = request.PaymentStatus;
            payload["search"] = request.Search;
            payload["limit"] = request.Limit <= 0 ? 50 : Math.Min(request.Limit, 200);
            payload["offset"] = Math.Max(request.Offset, 0);
        }
        return JsonSerializer.Serialize(payload);
    }

    private static string BuildBillPayload(RecordPaymentRequest request)
    {
        var lines = (request.Lines ?? [])
            .Select(l => new Dictionary<string, object?>
            {
                ["product_id"] = l.ProductId,
                ["description"] = l.Description,
                ["amount"] = l.Amount,
                ["cgst_percent"] = l.CgstPercent,
                ["sgst_percent"] = l.SgstPercent,
                ["igst_percent"] = l.IgstPercent
            })
            .ToList();

        var payload = new Dictionary<string, object?>
        {
            ["gst_mode"] = request.GstMode,
            ["gst_percent"] = request.GstPercent,
            ["bill_date"] = request.BillDate?.ToString("yyyy-MM-dd"),
            ["lines"] = lines,
            ["amount_received"] = request.AmountReceived,
            ["payment_method"] = request.PaymentMethod,
            ["payment_note"] = request.PaymentNote
        };

        return JsonSerializer.Serialize(payload);
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? appointmentId = null,
        string? payload = null,
        long? createdBy = null,
        int? fiscalYearId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(appointmentId),
            Jsonb(payload),
            Bigint(createdBy),
            NullableInt(fiscalYearId));

    private static AppointmentPaymentDto DeserializeRequired(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            throw new InvalidOperationException($"{Fn} returned no data");
        return JsonSerializer.Deserialize<AppointmentPaymentDto>(json, PostgresJsonOptions.Options)
               ?? throw new InvalidOperationException($"{Fn} returned empty payment payload");
    }

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

    private static NpgsqlParameter Jsonb(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value,
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };
}
