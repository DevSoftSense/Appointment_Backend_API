using System.Text.Json;
using Appointment.Domain.DTOs.Settings.Requests;
using Appointment.Domain.DTOs.Settings.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_settings(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class SettingsRepository : ISettingsRepository
{
    private const string Fn = "appointment.fn_appointment_settings";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<SettingsRepository> _logger;

    public SettingsRepository(ProductDatabaseHelper db, ILogger<SettingsRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<SettingsMasterSummaryDto>> GetMastersSummaryAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("masters_summary", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<SettingsMasterSummaryDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} masters_summary for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize masters_summary for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Settings masters summary could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<SettingsLookupItemDto>> GetLookupsAsync(
        int orgId, int appId, GetLookupsRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list_lookups",
                orgId,
                appId,
                lookupKind: request.LookupKind,
                includeInactive: request.IncludeInactive,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<SettingsLookupItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list_lookups for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize list_lookups for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Settings lookups could not be parsed.", ex);
        }
    }

    public async Task<SettingsLookupItemDto> CreateLookupAsync(
        int orgId, int appId, SaveLookupRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create_lookup",
                orgId,
                appId,
                lookupKind: request.LookupKind,
                lookupCode: request.LookupCode,
                lookupName: request.LookupName,
                sortOrder: request.SortOrder,
                isActive: request.IsActive);

            return JsonSerializer.Deserialize<SettingsLookupItemDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create lookup returned empty response.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create_lookup for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize create_lookup for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Create lookup response could not be parsed.", ex);
        }
    }

    public async Task<SettingsLookupItemDto> UpdateLookupAsync(
        int orgId, int appId, int lookupId, UpdateLookupRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update_lookup",
                orgId,
                appId,
                lookupId: lookupId,
                lookupCode: request.LookupCode,
                lookupName: request.LookupName,
                sortOrder: request.SortOrder,
                isActive: request.IsActive);

            return JsonSerializer.Deserialize<SettingsLookupItemDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update lookup returned empty response.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update_lookup for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize update_lookup for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Update lookup response could not be parsed.", ex);
        }
    }

    public async Task DeactivateLookupAsync(
        int orgId, int appId, int lookupId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            await CallAsync("deactivate_lookup", orgId, appId, lookupId: lookupId);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate_lookup for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    public async Task<AppointmentRulesDto> GetRulesAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_rules", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new AppointmentRulesDto();

            return JsonSerializer.Deserialize<AppointmentRulesDto>(json, PostgresJsonOptions.Options)
                   ?? new AppointmentRulesDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_rules for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize get_rules for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Appointment rules could not be parsed.", ex);
        }
    }

    public async Task<AppointmentRulesDto> SetRulesAsync(
        int orgId, int appId, SaveAppointmentRulesRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var payload = new Dictionary<string, object?>();
            if (request.AutoNoShowEnabled.HasValue)
                payload["auto_no_show_enabled"] = request.AutoNoShowEnabled.Value;
            if (request.AutoNoShowGraceMinutes.HasValue)
                payload["auto_no_show_grace_minutes"] = request.AutoNoShowGraceMinutes.Value;

            var rulesJson = JsonSerializer.Serialize(payload, PostgresJsonOptions.Options);
            var json = await CallAsync("set_rules", orgId, appId, rulesJson: rulesJson);

            return JsonSerializer.Deserialize<AppointmentRulesDto>(json, PostgresJsonOptions.Options)
                   ?? new AppointmentRulesDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} set_rules for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize set_rules for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Save appointment rules response could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        string? lookupKind = null,
        int? lookupId = null,
        string? lookupCode = null,
        string? lookupName = null,
        int? sortOrder = null,
        bool? isActive = null,
        bool? includeInactive = null,
        int? limit = null,
        int? offset = null,
        string? rulesJson = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            Varchar(lookupKind),
            NullableInt(lookupId),
            Varchar(lookupCode),
            Varchar(lookupName),
            NullableInt(sortOrder),
            Bool(isActive),
            Bool(includeInactive),
            NullableInt(limit),
            NullableInt(offset),
            Jsonb(rulesJson));

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

    private static NpgsqlParameter Bool(bool? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Boolean
        };

    private static NpgsqlParameter Jsonb(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value,
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };
}
