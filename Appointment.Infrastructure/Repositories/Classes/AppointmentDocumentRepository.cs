using System.Text.Json;
using Appointment.Domain.DTOs.Appointments.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class AppointmentDocumentRepository : IAppointmentDocumentRepository
{
    private const string Fn = "appointment.fn_appointment_document";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<AppointmentDocumentRepository> _logger;

    public AppointmentDocumentRepository(
        ProductDatabaseHelper db,
        ILogger<AppointmentDocumentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AppointmentDocumentDto>> ListAsync(
        int orgId, int appId, long appointmentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("list", orgId, appId, appointmentId: appointmentId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<AppointmentDocumentDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} list", Fn);
            throw;
        }
    }

    public async Task<IReadOnlyList<AppointmentDocumentDto>> ListByCustomerAsync(
        int orgId, int appId, long customerId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("list_by_customer", orgId, appId, customerId: customerId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<AppointmentDocumentDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} list_by_customer", Fn);
            throw;
        }
    }

    public async Task<AppointmentDocumentDto?> GetByIdAsync(
        int orgId, int appId, long documentId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, documentId: documentId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;
            return JsonSerializer.Deserialize<AppointmentDocumentDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} get_by_id", Fn);
            throw;
        }
    }

    public async Task<AppointmentDocumentDto> CreateAsync(
        int orgId,
        int appId,
        long appointmentId,
        long uploadedBy,
        string documentName,
        string filePath,
        string? documentType,
        int? fiscalYearId = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                appointmentId: appointmentId,
                documentName: documentName,
                filePath: filePath,
                documentType: documentType,
                uploadedBy: uploadedBy,
                fiscalYearId: fiscalYearId);

            return JsonSerializer.Deserialize<AppointmentDocumentDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} create returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create", Fn);
            throw;
        }
    }

    public async Task<(bool Deleted, string? FilePath)> DeleteAsync(
        int orgId,
        int appId,
        long appointmentId,
        long documentId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "delete",
                orgId,
                appId,
                appointmentId: appointmentId,
                documentId: documentId);

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var deleted = root.TryGetProperty("deleted", out var d) && d.GetBoolean();
            string? path = null;
            if (root.TryGetProperty("file_path", out var fp) && fp.ValueKind == JsonValueKind.String)
                path = fp.GetString();
            return (deleted, path);
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} delete", Fn);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? appointmentId = null,
        long? documentId = null,
        string? documentName = null,
        string? filePath = null,
        string? documentType = null,
        long? uploadedBy = null,
        int? fiscalYearId = null,
        long? customerId = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(appointmentId),
            Bigint(documentId),
            Varchar(documentName),
            Varchar(filePath),
            Varchar(documentType),
            Bigint(uploadedBy),
            Int(fiscalYearId),
            Bigint(customerId));

    private static NpgsqlParameter Int(int? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Integer, Value = value.HasValue ? value.Value : DBNull.Value };

    private static NpgsqlParameter Bigint(long? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Bigint, Value = value.HasValue ? value.Value : DBNull.Value };

    private static NpgsqlParameter Varchar(string? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value };
}
