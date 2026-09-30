using System.Text.Json;
using Appointment.Domain.DTOs.Permissions.Requests;
using Appointment.Domain.DTOs.Permissions.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

public sealed class PermissionRepository : IPermissionRepository
{
    private const string Fn = "appointment.fn_appointment_permission";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<PermissionRepository> _logger;

    public PermissionRepository(ProductDatabaseHelper db, ILogger<PermissionRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PermissionModuleDto>> ListModulesAsync(
        int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync("list_modules", orgId, appId);
        return DeserializeList(json);
    }

    public async Task<IReadOnlyList<PermissionModuleDto>> GetRoleAsync(
        int orgId, int appId, string roleCode, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync("get_role", orgId, appId, roleCode: roleCode);
        return DeserializeList(json);
    }

    public async Task<IReadOnlyList<PermissionModuleDto>> SaveRoleAsync(
        int orgId, int appId, string roleCode, SaveRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var payload = JsonSerializer.Serialize(
            (request.Items ?? []).Select(i => new
            {
                menu_code = i.MenuCode,
                can_view = i.CanView,
                can_add = i.CanAdd,
                can_edit = i.CanEdit,
                can_delete = i.CanDelete
            }),
            PostgresJsonOptions.Options);

        var json = await CallAsync(
            "save_role", orgId, appId, roleCode: roleCode, payloadJson: payload);
        return DeserializeList(json);
    }

    public async Task<MyPermissionsDto> GetMyAsync(
        int orgId, int appId, IReadOnlyList<string> roleCodes, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        var json = await CallAsync(
            "get_my",
            orgId,
            appId,
            isAdmin: isAdmin,
            roleCodes: roleCodes?.ToArray());

        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return new MyPermissionsDto();

        return JsonSerializer.Deserialize<MyPermissionsDto>(json, PostgresJsonOptions.Options)
               ?? new MyPermissionsDto();
    }

    public async Task SeedPresetsAsync(int orgId, int appId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        await CallAsync("seed_presets", orgId, appId);
    }

    private async Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        string? roleCode = null,
        bool isAdmin = false,
        string? payloadJson = null,
        string[]? roleCodes = null)
    {
        try
        {
            return await _db.ExecuteJsonFunctionAsync(
                Fn,
                Varchar(action),
                Int(orgId),
                Int(appId),
                Varchar(roleCode),
                Bool(isAdmin),
                Jsonb(payloadJson),
                TextArray(roleCodes));
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} action={Action}", Fn, action);
            throw;
        }
    }

    private static IReadOnlyList<PermissionModuleDto> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || json == "null")
            return [];
        return JsonSerializer.Deserialize<List<PermissionModuleDto>>(json, PostgresJsonOptions.Options)
               ?? [];
    }

    private static NpgsqlParameter Int(int value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Integer };

    private static NpgsqlParameter Varchar(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value.Trim(),
            NpgsqlDbType = NpgsqlDbType.Varchar
        };

    private static NpgsqlParameter Bool(bool value) =>
        new() { Value = value, NpgsqlDbType = NpgsqlDbType.Boolean };

    private static NpgsqlParameter Jsonb(string? json) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(json) ? DBNull.Value : json,
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };

    private static NpgsqlParameter TextArray(string[]? values) =>
        new()
        {
            Value = values is { Length: > 0 } ? values : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text
        };
}
