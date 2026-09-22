using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Appointment.Infrastructure.Repositories.Interfaces;
using Npgsql;
using System.Text;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// Executes PostgreSQL functions on the organisation product transaction DB.
/// <para>
/// SoftOnCloud:UseProductConnectionDb = false → local SecondConnection (dev)
/// SoftOnCloud:UseProductConnectionDb = true  →
///   staff JWT → GET /api/auth/product-connection;
///   public QR (IPublicBookOrgContext) → GET /api/auth/product-connection/service + ProductServiceKey;
///   workers / emergency → SecondConnection fallback
/// </para>
/// </summary>
public sealed class ProductDatabaseHelper
{
    private readonly ITenantConnectionFactory _tenantConnections;
    private readonly IPublicBookOrgContext _publicBookOrg;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly bool _useProductConnectionApi;
    private readonly string? _localConnectionString;
    private readonly string? _productServiceKey;
    private readonly ILogger<ProductDatabaseHelper> _logger;

    public ProductDatabaseHelper(
        ITenantConnectionFactory tenantConnections,
        IPublicBookOrgContext publicBookOrg,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<ProductDatabaseHelper> logger)
    {
        _tenantConnections = tenantConnections;
        _publicBookOrg = publicBookOrg;
        _httpContextAccessor = httpContextAccessor;
        _useProductConnectionApi = configuration.GetValue<bool>("SoftOnCloud:UseProductConnectionDb");
        _localConnectionString = configuration.GetConnectionString("SecondConnection")
                                 ?? configuration.GetConnectionString("DefaultConnection");
        _productServiceKey = configuration["SoftOnCloud:ProductServiceKey"];
        _logger = logger;
    }

    /// <summary>
    /// Calls a PostgreSQL function that already returns JSONB/JSON (e.g. unified module fn with p_action).
    /// </summary>
    public async Task<string> ExecuteJsonFunctionAsync(
        string functionName,
        params NpgsqlParameter[] parameters)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(functionName);
        ValidateFunctionName(functionName);

        var sql = BuildJsonFunctionSql(functionName, parameters.Length);

        try
        {
            await using var connection = await OpenConnectionAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            if (parameters.Length > 0)
                command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();
            return result is null or DBNull ? "null" : result.ToString() ?? "null";
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error executing JSON function {FunctionName}. SqlState={SqlState}",
                functionName, ex.SqlState);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error executing JSON function {FunctionName}", functionName);
            throw;
        }
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
            await using var connection = await OpenConnectionAsync();
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
            await using var connection = await OpenConnectionAsync();
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

    private async Task<NpgsqlConnection> OpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = await ResolveConnectionStringAsync(cancellationToken);
        var connection = new NpgsqlConnection(connectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private async Task<string> ResolveConnectionStringAsync(CancellationToken cancellationToken)
    {
        // Local/dev: functions live on local SOC_SaaS_Product until deployed to SoftOnCloud live.
        if (!_useProductConnectionApi)
        {
            if (string.IsNullOrWhiteSpace(_localConnectionString))
            {
                throw new InvalidOperationException(
                    "SoftOnCloud:UseProductConnectionDb is false, but ConnectionStrings:SecondConnection is not set. " +
                    "Add your local product DB connection string.");
            }

            return _localConnectionString;
        }

        // Live: staff JWT → SoftOnCloud product-connection.
        if (HasSoftOnCloudBearer())
        {
            var connectionString = await _tenantConnections.GetResolvedConnectionAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "SoftOnCloud product-connection returned an empty connection string.");
            }

            return connectionString;
        }

        // Live: public QR — org from decrypted token t + product service key (no staff JWT).
        if (_publicBookOrg.OrgId is int publicOrgId && publicOrgId > 0
            && !string.IsNullOrWhiteSpace(_productServiceKey))
        {
            var connectionString = await _tenantConnections.GetResolvedConnectionForServiceAsync(
                publicOrgId,
                cancellationToken);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "SoftOnCloud product-connection/service returned an empty connection string.");
            }

            return connectionString;
        }

        // Workers / emergency fallback (reminders, auto no-show) — no JWT and no public org context.
        if (!string.IsNullOrWhiteSpace(_localConnectionString))
        {
            _logger.LogWarning(
                "No SoftOnCloud JWT and no public-book org/service-key path; using ConnectionStrings:SecondConnection " +
                "(expected for background workers). PublicOrgId={PublicOrgId} HasServiceKey={HasServiceKey}",
                _publicBookOrg.OrgId,
                !string.IsNullOrWhiteSpace(_productServiceKey));
            return _localConnectionString;
        }

        if (_publicBookOrg.OrgId is > 0 && string.IsNullOrWhiteSpace(_productServiceKey))
        {
            throw new InvalidOperationException(
                "Public QR booking requires SoftOnCloud:ProductServiceKey on the API server " +
                "(value issued by SoftOnCloud for Appointment product-connection/service).");
        }

        throw new InvalidOperationException(
            "Cannot resolve product DB: no SoftOnCloud JWT, no public-book org + ProductServiceKey, " +
            "and ConnectionStrings:SecondConnection is not set.");
    }

    private bool HasSoftOnCloudBearer()
    {
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        return !string.IsNullOrWhiteSpace(authHeader)
               && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildJsonFunctionSql(string functionName, int paramCount)
    {
        var sb = new StringBuilder();
        sb.Append("SELECT ").Append(functionName).Append('(');
        AppendPositionalParams(sb, paramCount);
        sb.Append(")::text");
        return sb.ToString();
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
