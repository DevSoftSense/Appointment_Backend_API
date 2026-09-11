using System.Text.Json;
using Appointment.Domain.DTOs.Cabins.Requests;
using Appointment.Domain.DTOs.Cabins.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_cabin(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class CabinRepository : ICabinRepository
{
    private const string Fn = "appointment.fn_appointment_cabin";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<CabinRepository> _logger;

    public CabinRepository(ProductDatabaseHelper db, ILogger<CabinRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<CabinListItemDto>> GetCabinsAsync(
        int orgId,
        int appId,
        GetCabinsRequest request,
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
                isActive: request.IsActive,
                branchId: request.BranchId,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<CabinListItemDto>>(json, PostgresJsonOptions.Options)
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
            throw new InvalidOperationException("Cabin list response could not be parsed.", ex);
        }
    }

    public async Task<CabinDetailDto?> GetCabinByIdAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, cabinResourceId: cabinResourceId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<CabinDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for cabinId {CabinId}", Fn, cabinResourceId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for cabinId {CabinId}", Fn, cabinResourceId);
            throw new InvalidOperationException("Cabin response could not be parsed.", ex);
        }
    }

    public async Task<CabinDetailDto> CreateCabinAsync(
        int orgId,
        int appId,
        CreateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "create",
                orgId,
                appId,
                fiscalYearId: request.FiscalYearId,
                branchId: request.BranchId,
                resourceCode: request.ResourceCode,
                resourceName: request.ResourceName,
                resourceType: request.ResourceType,
                description: request.Description,
                setIsActive: request.IsActive ?? true);

            return JsonSerializer.Deserialize<CabinDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} create returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} create for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} create for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Create cabin response could not be parsed.", ex);
        }
    }

    public async Task<CabinDetailDto> UpdateCabinAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        UpdateCabinRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                cabinResourceId: cabinResourceId,
                fiscalYearId: request.FiscalYearId,
                branchId: request.BranchId,
                resourceName: request.ResourceName,
                resourceType: request.ResourceType,
                description: request.Description,
                setIsActive: request.IsActive);

            return JsonSerializer.Deserialize<CabinDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} update returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} update for cabinId {CabinId}", Fn, cabinResourceId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} update for cabinId {CabinId}", Fn, cabinResourceId);
            throw new InvalidOperationException("Update cabin response could not be parsed.", ex);
        }
    }

    public async Task<CabinDetailDto> DeactivateCabinAsync(
        int orgId,
        int appId,
        long cabinResourceId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("deactivate", orgId, appId, cabinResourceId: cabinResourceId);

            return JsonSerializer.Deserialize<CabinDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} deactivate returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} deactivate for cabinId {CabinId}", Fn, cabinResourceId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} deactivate for cabinId {CabinId}", Fn, cabinResourceId);
            throw new InvalidOperationException("Deactivate cabin response could not be parsed.", ex);
        }
    }

    public async Task<CabinStatsDto> GetStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("stats", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new CabinStatsDto();

            return JsonSerializer.Deserialize<CabinStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new CabinStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize cabin stats for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Cabin stats could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<CabinBranchDto>> GetBranchesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("branches", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<CabinBranchDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} branches for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize cabin branches for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Branch list could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        long? cabinResourceId = null,
        string? search = null,
        bool? isActive = null,
        int? limit = null,
        int? offset = null,
        int? fiscalYearId = null,
        int? branchId = null,
        string? resourceCode = null,
        string? resourceName = null,
        string? resourceType = null,
        string? description = null,
        bool? setIsActive = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Bigint(cabinResourceId),
            Varchar(search),
            Bool(isActive),
            NullableInt(limit),
            NullableInt(offset),
            NullableInt(fiscalYearId),
            NullableInt(branchId),
            Varchar(resourceCode),
            Varchar(resourceName),
            Varchar(resourceType),
            Text(description),
            Bool(setIsActive));

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

    private static NpgsqlParameter Bool(bool? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Boolean
        };
}
