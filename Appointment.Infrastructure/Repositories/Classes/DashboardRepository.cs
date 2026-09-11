using System.Text.Json;
using Appointment.Domain.DTOs.Dashboard.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class DashboardRepository : IDashboardRepository
{
    private const string Fn = "appointment.fn_appointment_dashboard";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(ProductDatabaseHelper db, ILogger<DashboardRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DashboardOverviewDto> GetOverviewAsync(
        int orgId,
        int appId,
        DateOnly? asOfDate,
        int? branchId,
        int? professionalId,
        string? period = null,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteJsonFunctionAsync(
                Fn,
                Varchar("overview"),
                Int(orgId),
                Int(appId),
                Date(asOfDate),
                NullableInt(branchId),
                NullableInt(professionalId),
                Varchar(string.IsNullOrWhiteSpace(period) ? "today" : period));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new DashboardOverviewDto();

            return JsonSerializer.Deserialize<DashboardOverviewDto>(json, PostgresJsonOptions.Options)
                   ?? new DashboardOverviewDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} overview for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} overview for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Dashboard overview response could not be parsed.", ex);
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

    private static NpgsqlParameter Date(DateOnly? value) =>
        new()
        {
            Value = value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
