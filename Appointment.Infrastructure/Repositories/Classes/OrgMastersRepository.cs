using System.Text.Json;
using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.OrgMasters.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_org_masters(p_action, …). Branch CRUD only. No inline table SQL.
/// </summary>
public sealed class OrgMastersRepository : IOrgMastersRepository
{
    private const string Fn = "appointment.fn_appointment_org_masters";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<OrgMastersRepository> _logger;

    public OrgMastersRepository(ProductDatabaseHelper db, ILogger<OrgMastersRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<BranchDetailDto>> ListBranchesAsync(
        int orgId, int appId, GetBranchesRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "list_branches",
                orgId,
                appId,
                includeInactive: request.IncludeInactive,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<BranchDetailDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list_branches for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize list_branches for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Branch list could not be parsed.", ex);
        }
    }

    public async Task<BranchDetailDto> GetBranchAsync(
        int orgId, int appId, int branchId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync("get_branch", orgId, appId, branchId: branchId);
            return JsonSerializer.Deserialize<BranchDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Branch not found.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_branch for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize get_branch for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Branch could not be parsed.", ex);
        }
    }

    public async Task<BranchDetailDto> CreateBranchAsync(
        int orgId, int appId, SaveBranchRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "create_branch",
                orgId,
                appId,
                branchName: request.BranchName,
                branchCode: request.BranchCode,
                phone: request.Phone,
                email: request.Email,
                timezone: request.Timezone,
                address: request.Address,
                city: request.City,
                state: request.State,
                country: request.Country,
                pincode: request.Pincode,
                isActive: request.IsActive);

            return JsonSerializer.Deserialize<BranchDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Create branch returned empty response.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} create_branch for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize create_branch for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Create branch response could not be parsed.", ex);
        }
    }

    public async Task<BranchDetailDto> UpdateBranchAsync(
        int orgId, int appId, int branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            var json = await CallAsync(
                "update_branch",
                orgId,
                appId,
                branchId: branchId,
                branchName: request.BranchName,
                branchCode: request.BranchCode,
                phone: request.Phone,
                email: request.Email,
                timezone: request.Timezone,
                address: request.Address,
                city: request.City,
                state: request.State,
                country: request.Country,
                pincode: request.Pincode,
                isActive: request.IsActive);

            return JsonSerializer.Deserialize<BranchDetailDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Update branch returned empty response.");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} update_branch for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize update_branch for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Update branch response could not be parsed.", ex);
        }
    }

    public async Task DeactivateBranchAsync(
        int orgId, int appId, int branchId, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        try
        {
            await CallAsync("deactivate_branch", orgId, appId, branchId: branchId);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} deactivate_branch for orgId {OrgId}", Fn, orgId);
            throw;
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? branchId = null,
        string? branchName = null,
        string? branchCode = null,
        string? phone = null,
        string? email = null,
        string? timezone = null,
        string? address = null,
        string? city = null,
        string? state = null,
        string? country = null,
        string? pincode = null,
        bool? isActive = null,
        bool? includeInactive = null,
        int? limit = null,
        int? offset = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(branchId),
            Varchar(branchName),
            Varchar(branchCode),
            Varchar(phone),
            Varchar(email),
            Varchar(timezone),
            Text(address),
            Varchar(city),
            Varchar(state),
            Varchar(country),
            Varchar(pincode),
            Bool(isActive),
            Bool(includeInactive),
            NullableInt(limit),
            NullableInt(offset));

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
