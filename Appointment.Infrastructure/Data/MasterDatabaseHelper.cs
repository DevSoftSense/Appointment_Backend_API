using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// Infrastructure helper for executing PostgreSQL functions and returning raw results.
/// Connects to SOC_SaaS_V2 (MasterConnection) for auth/login functions.
/// Must not contain business-specific methods or DTO deserialization.
/// </summary>
public sealed class MasterDatabaseHelper
{
    private readonly string _connectionString;
    private readonly ILogger<MasterDatabaseHelper> _logger;

    public MasterDatabaseHelper(IConfiguration configuration, ILogger<MasterDatabaseHelper> logger)
    {
        _connectionString = configuration.GetConnectionString("MasterConnection")
            ?? throw new InvalidOperationException("Connection string 'MasterConnection' is not configured.");
        _logger = logger;
    }

    /// <summary>
    /// Executes a PostgreSQL function that returns JSON (scalar) and returns the raw JSON string.
    /// Used for fn_resolve_user_context which returns JSON directly.
    /// </summary>
    public async Task<string> ExecuteJsonFunctionAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildFunctionCallSql(functionName, parameters.Length);

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

    /// <summary>
    /// Executes a PostgreSQL function that returns a table and converts the first row to JSON.
    /// Used for fn_get_user_for_org_login, fn_record_login_attempt, fn_create_user_session,
    /// fn_get_org_login_context.
    /// </summary>
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

    public async Task<string?> ExecuteScalarSqlAsync(
        string sql,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? null : result.ToString();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error executing scalar SQL. SqlState={SqlState}", ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing scalar SQL");
            throw;
        }
    }

    // ─── SQL builders ────────────────────────────────────────────────────────

    private static string BuildFunctionCallSql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ").Append(functionName).Append('(');
        for (var i = 0; i < paramCount; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('$').Append(i + 1);
        }
        sb.Append(')');
        return sb.ToString();
    }

    private static string BuildSingleRowTableFunctionToJsonSql(string functionName, int paramCount)
    {
        // SELECT row_to_json(t)::text
        // FROM (SELECT * FROM public.fn_name($1,$2,...)) AS t
        // LIMIT 1
        var sb = new StringBuilder();
        sb.Append("SELECT row_to_json(t)::text ");
        sb.Append("FROM (SELECT * FROM ").Append(functionName).Append('(');
        for (var i = 0; i < paramCount; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('$').Append(i + 1);
        }
        sb.Append(")) AS t LIMIT 1");
        return sb.ToString();
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
