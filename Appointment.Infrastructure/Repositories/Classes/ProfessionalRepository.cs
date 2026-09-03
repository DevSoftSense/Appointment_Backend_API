using System.Text.Json;
using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;
using Appointment.Infrastructure.Data;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls appointment.fn_appointment_* on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class ProfessionalRepository : IProfessionalRepository
{
    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ProfessionalRepository> _logger;

    public ProfessionalRepository(ProductDatabaseHelper db, ILogger<ProfessionalRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProfessionalListItemDto>> GetProfessionalsAsync(
        int orgId,
        int productId,
        GetProfessionalsRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_professionals",
                Int(orgId),
                Int(productId),
                Varchar(request.Search),
                Varchar(request.Status),
                Int(request.Limit),
                Int(request.Offset));

            return JsonSerializer.Deserialize<List<ProfessionalListItemDto>>(json, PostgresJsonOptions.Options)
                   ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_professionals for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_professionals for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Professional list response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int productId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_get_professional_by_id",
                Int(orgId),
                Int(productId),
                Int(employeeId));

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<ProfessionalDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_professional_by_id for employeeId {EmployeeId}", employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_get_professional_by_id for employeeId {EmployeeId}", employeeId);
            throw new InvalidOperationException("Professional response could not be parsed.", ex);
        }
    }

    public async Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        int productId,
        CreateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "appointment.fn_appointment_create_professional",
                Int(orgId),
                Int(productId),
                NullableInt(request.FiscalYearId),
                Varchar(request.FullName),
                Varchar(request.Email),
                Varchar(request.Phone),
                Varchar(request.Gender),
                Date(request.DateOfBirth),
                Date(request.JoinDate),
                Varchar(request.EmploymentType),
                NullableInt(request.BranchId),
                NullableInt(request.DepartmentId),
                NullableInt(request.StaffRoleId),
                Varchar(request.Address),
                Varchar(string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status));

            return JsonSerializer.Deserialize<CreateProfessionalResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_appointment_create_professional returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in fn_appointment_create_professional for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_appointment_create_professional for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Create professional response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_branches",
                Int(orgId),
                Int(productId));

            return JsonSerializer.Deserialize<List<BranchDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_branches for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize branches for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Branch list could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int productId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_departments",
                Int(orgId),
                Int(productId),
                NullableInt(branchId));

            return JsonSerializer.Deserialize<List<DepartmentDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_departments for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize departments for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Department list could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int productId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteTableFunctionAsJsonArrayAsync(
                "appointment.fn_appointment_get_professional_roles",
                Int(orgId),
                Int(productId),
                NullableInt(branchId));

            return JsonSerializer.Deserialize<List<ProfessionalRoleDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_appointment_get_professional_roles for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize roles for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Role list could not be parsed.", ex);
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
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
