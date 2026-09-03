using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// Executes PostgreSQL functions on SOC_SaaS_Product (DefaultConnection).
/// Appointment module repositories call this — no business DTOs here.
/// </summary>
public sealed class ProductDatabaseHelper
{
    private readonly string _connectionString;
    private readonly ILogger<ProductDatabaseHelper> _logger;

    public ProductDatabaseHelper(IConfiguration configuration, ILogger<ProductDatabaseHelper> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        _logger = logger;
    }

    public async Task<string> ExecuteSingleRowTableFunctionAsJsonAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildSingleRowTableFunctionToJsonSql(functionName, parameters.Length);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "null" : result.ToString() ?? "null";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing function {FunctionName}", functionName);
            throw;
        }
    }

    public async Task<string> ExecuteTableFunctionAsJsonArrayAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildTableFunctionToJsonArraySql(functionName, parameters.Length);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "[]" : result.ToString() ?? "[]";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing function {FunctionName}", functionName);
            throw;
        }
    }

    private static string BuildSingleRowTableFunctionToJsonSql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT row_to_json(t)::text ");
        sb.Append("FROM (SELECT * FROM ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")) AS t LIMIT 1");
        return sb.ToString();
    }

    private static string BuildTableFunctionToJsonArraySql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT COALESCE(json_agg(t), '[]'::json)::text ");
        sb.Append("FROM (SELECT * FROM ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")) AS t");
        return sb.ToString();
    }

    private static void AppendPositionalParams(StringBuilder sb, int paramCount)
    {
        for (var i = 0; i < paramCount; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('$').Append(i + 1);
        }
    }

    private static void ValidateFunctionName(string functionName)
    {
        foreach (var ch in functionName)
        {
            if (!(char.IsLetterOrDigit(ch) || ch is '_' or '.'))
                throw new ArgumentException($"Invalid PostgreSQL function name: {functionName}", nameof(functionName));
        }
    }
}
