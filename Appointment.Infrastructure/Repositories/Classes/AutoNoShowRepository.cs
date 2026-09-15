using System.Text.Json;
using Appointment.Domain.DTOs.Settings.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class AutoNoShowRepository : IAutoNoShowRepository
{
    private const string Fn = "appointment.fn_appointment_auto_no_show";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<AutoNoShowRepository> _logger;

    public AutoNoShowRepository(ProductDatabaseHelper db, ILogger<AutoNoShowRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<AutoNoShowSweepResultDto> SweepAsync(
        int appId, int? orgId, int batchSize, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await _db.ExecuteJsonFunctionAsync(
                Fn,
                Varchar("sweep"),
                Int(appId),
                Int(orgId),
                Int(batchSize));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new AutoNoShowSweepResultDto();

            return JsonSerializer.Deserialize<AutoNoShowSweepResultDto>(json, PostgresJsonOptions.Options)
                   ?? new AutoNoShowSweepResultDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} sweep", Fn);
            throw;
        }
    }

    private static NpgsqlParameter Varchar(string? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Varchar, Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value };

    private static NpgsqlParameter Int(int? value) =>
        new() { NpgsqlDbType = NpgsqlDbType.Integer, Value = value.HasValue ? value.Value : DBNull.Value };
}
