using System.Text.Json;
using Appointment.Domain.DTOs.Reports.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class ReportsRepository : IReportsRepository
{
    private const string Fn = "appointment.fn_appointment_report";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ReportsRepository> _logger;

    public ReportsRepository(ProductDatabaseHelper db, ILogger<ReportsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public Task<ReportOverviewDto> GetOverviewAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default) =>
        CallAsync<ReportOverviewDto>("overview", orgId, appId, fromDate, toDate, branchId, professionalId,
            cancellationToken: cancellationToken);

    public Task<ReportAppointmentsDto> GetAppointmentsAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, string? status,
        int limit, int offset, CancellationToken cancellationToken = default) =>
        CallAsync<ReportAppointmentsDto>("appointments", orgId, appId, fromDate, toDate, branchId, professionalId,
            limit, offset, status, cancellationToken);

    public Task<ReportServicesDto> GetServicesAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default) =>
        CallAsync<ReportServicesDto>("services", orgId, appId, fromDate, toDate, branchId, professionalId,
            cancellationToken: cancellationToken);

    public Task<ReportProfessionalsDto> GetProfessionalsAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default) =>
        CallAsync<ReportProfessionalsDto>("professionals", orgId, appId, fromDate, toDate, branchId, professionalId,
            cancellationToken: cancellationToken);

    public Task<ReportNoShowDto> GetNoShowAsync(
        int orgId, int appId, DateOnly fromDate, DateOnly toDate,
        int? branchId, int? professionalId, CancellationToken cancellationToken = default) =>
        CallAsync<ReportNoShowDto>("no_show", orgId, appId, fromDate, toDate, branchId, professionalId,
            cancellationToken: cancellationToken);

    private async Task<T> CallAsync<T>(
        string action,
        int orgId,
        int appId,
        DateOnly fromDate,
        DateOnly toDate,
        int? branchId,
        int? professionalId,
        int? limit = null,
        int? offset = null,
        string? status = null,
        CancellationToken cancellationToken = default) where T : class, new()
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteJsonFunctionAsync(
                Fn,
                Varchar(action),
                Int(orgId),
                Int(appId),
                Date(fromDate),
                Date(toDate),
                NullableInt(branchId),
                NullableInt(professionalId),
                NullableInt(limit),
                NullableInt(offset),
                Varchar(status));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new T();

            return JsonSerializer.Deserialize<T>(json, PostgresJsonOptions.Options) ?? new T();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} {Action} for orgId {OrgId}", Fn, action, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} {Action} for orgId {OrgId}", Fn, action, orgId);
            throw new InvalidOperationException($"Report {action} response could not be parsed.", ex);
        }
    }

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter NullableInt(int? value) =>
        new() { Value = value.HasValue ? value.Value : DBNull.Value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Date(DateOnly value) =>
        new()
        {
            Value = value.ToDateTime(TimeOnly.MinValue),
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
