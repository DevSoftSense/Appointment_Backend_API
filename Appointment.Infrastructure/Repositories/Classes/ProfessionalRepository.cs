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
/// Calls appointment.fn_appointment_professional(p_action, …) on SOC_SaaS_Product. No inline table SQL.
/// </summary>
public sealed class ProfessionalRepository : IProfessionalRepository
{
    private const string Fn = "appointment.fn_appointment_professional";

    private readonly ProductDatabaseHelper _db;
    private readonly ILogger<ProfessionalRepository> _logger;

    public ProfessionalRepository(ProductDatabaseHelper db, ILogger<ProfessionalRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ProfessionalListItemDto>> GetProfessionalsAsync(
        int orgId,
        int appId,
        GetProfessionalsRequest request,
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
                status: request.Status,
                limit: request.Limit,
                offset: request.Offset);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];

            return JsonSerializer.Deserialize<List<ProfessionalListItemDto>>(json, PostgresJsonOptions.Options)
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
            throw new InvalidOperationException("Professional list response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("get_by_id", orgId, appId, employeeId: employeeId);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<ProfessionalDetailDto>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} get_by_id for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} get_by_id for employeeId {EmployeeId}", Fn, employeeId);
            throw new InvalidOperationException("Professional response could not be parsed.", ex);
        }
    }

    public async Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        int appId,
        CreateProfessionalRequest request,
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
                fullName: request.FullName,
                email: request.Email,
                phone: request.Phone,
                gender: request.Gender,
                dateOfBirth: request.DateOfBirth,
                joinDate: request.JoinDate,
                employmentType: request.EmploymentType,
                branchId: request.BranchId,
                departmentId: request.DepartmentId,
                staffRoleId: request.StaffRoleId,
                address: request.Address,
                status: string.IsNullOrWhiteSpace(request.Status) ? "active" : request.Status);

            return JsonSerializer.Deserialize<CreateProfessionalResponse>(json, PostgresJsonOptions.Options)
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
            throw new InvalidOperationException("Create professional response could not be parsed.", ex);
        }
    }

    public async Task<CreateProfessionalResponse> UpdateProfessionalAsync(
        int orgId,
        int appId,
        int employeeId,
        UpdateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync(
                "update",
                orgId,
                appId,
                employeeId: employeeId,
                fullName: request.FullName,
                email: request.Email,
                phone: request.Phone,
                gender: request.Gender,
                dateOfBirth: request.DateOfBirth,
                joinDate: request.JoinDate,
                employmentType: request.EmploymentType,
                branchId: request.BranchId,
                departmentId: request.DepartmentId,
                staffRoleId: request.StaffRoleId,
                address: request.Address,
                status: request.Status);

            return JsonSerializer.Deserialize<CreateProfessionalResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} update returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} update for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} update for employeeId {EmployeeId}", Fn, employeeId);
            throw new InvalidOperationException("Update professional response could not be parsed.", ex);
        }
    }

    public async Task<CreateProfessionalResponse> DeactivateProfessionalAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("deactivate", orgId, appId, employeeId: employeeId);

            return JsonSerializer.Deserialize<CreateProfessionalResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException($"{Fn} deactivate returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} deactivate for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} deactivate for employeeId {EmployeeId}", Fn, employeeId);
            throw new InvalidOperationException("Deactivate professional response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalStatsDto> GetProfessionalStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("stats", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return new ProfessionalStatsDto();

            return JsonSerializer.Deserialize<ProfessionalStatsDto>(json, PostgresJsonOptions.Options)
                   ?? new ProfessionalStatsDto();
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize {Fn} stats for orgId {OrgId}", Fn, orgId);
            throw new InvalidOperationException("Professional stats response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> ListProfessionalServicesAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("list_services", orgId, appId, employeeId: employeeId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<ProfessionalServiceItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} list_services for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize list_services for employeeId {EmployeeId}", employeeId);
            throw new InvalidOperationException("Professional services response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> GetAvailableServicesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("available_services", orgId, appId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<ProfessionalServiceItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} available_services for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize available_services for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Available services response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> SetProfessionalServicesAsync(
        int orgId,
        int appId,
        int employeeId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var csv = string.Join(",", (productIds ?? []).Where(id => id > 0).Distinct());
            var json = await CallAsync(
                "set_services",
                orgId,
                appId,
                employeeId: employeeId,
                productIds: string.IsNullOrWhiteSpace(csv) ? null : csv);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<ProfessionalServiceItemDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} set_services for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize set_services for employeeId {EmployeeId}", employeeId);
            throw new InvalidOperationException("Set professional services response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("get_schedule", orgId, appId, employeeId: employeeId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Schedule response was empty.");

            return JsonSerializer.Deserialize<ProfessionalScheduleDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Schedule response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} get_schedule for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize get_schedule for employeeId {EmployeeId}", employeeId);
            throw new InvalidOperationException("Professional schedule response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalScheduleDto> SaveScheduleAsync(
        int orgId,
        int appId,
        int employeeId,
        SaveProfessionalScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var payload = new
            {
                days = (request.Days ?? []).Select(d => new
                {
                    dayOfWeek = d.DayOfWeek,
                    isClosed = d.IsClosed,
                    start = d.Start,
                    end = d.End,
                    breakStart = d.BreakStart,
                    breakEnd = d.BreakEnd
                }),
                consultDurationMinutes = request.ConsultDurationMinutes,
                bufferMinutes = request.BufferMinutes,
                timezone = request.Timezone,
                effectiveFrom = request.EffectiveFrom?.ToString("yyyy-MM-dd")
            };

            var scheduleJson = JsonSerializer.Serialize(payload, PostgresJsonOptions.Options);
            var json = await CallAsync(
                "save_schedule",
                orgId,
                appId,
                employeeId: employeeId,
                scheduleJson: scheduleJson);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Save schedule response was empty.");

            return JsonSerializer.Deserialize<ProfessionalScheduleDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Save schedule response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} save_schedule for employeeId {EmployeeId}", Fn, employeeId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize save_schedule for employeeId {EmployeeId}", employeeId);
            throw new InvalidOperationException("Save professional schedule response could not be parsed.", ex);
        }
    }

    public async Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        int appId,
        DateOnly fromDate,
        DateOnly toDate,
        IEnumerable<int> employeeIds,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var ids = (employeeIds ?? []).Where(id => id > 0).Distinct().ToList();
            var payload = new
            {
                from_date = fromDate.ToString("yyyy-MM-dd"),
                to_date = toDate.ToString("yyyy-MM-dd"),
                employee_ids = ids
            };
            var scheduleJson = JsonSerializer.Serialize(payload, PostgresJsonOptions.Options);

            var json = await CallAsync(
                "get_schedule_grid",
                orgId,
                appId,
                employeeId: ids.Count == 1 ? ids[0] : null,
                scheduleJson: scheduleJson);

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                throw new InvalidOperationException("Schedule grid response was empty.");

            return JsonSerializer.Deserialize<ProfessionalScheduleGridDto>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("Schedule grid response could not be parsed.");
        }
        catch (PostgresException ex)
        {
            _logger.LogWarning(ex, "PostgreSQL error in {Fn} get_schedule_grid for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize get_schedule_grid for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Professional schedule grid response could not be parsed.", ex);
        }
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
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
            return JsonSerializer.Deserialize<List<BranchDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} branches for orgId {OrgId}", Fn, orgId);
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
        int appId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("departments", orgId, appId, branchId: branchId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<DepartmentDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} departments for orgId {OrgId}", Fn, orgId);
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
        int appId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await CallAsync("roles", orgId, appId, branchId: branchId);
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return [];
            return JsonSerializer.Deserialize<List<ProfessionalRoleDto>>(json, PostgresJsonOptions.Options) ?? [];
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in {Fn} roles for orgId {OrgId}", Fn, orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize roles for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Role list could not be parsed.", ex);
        }
    }

    private Task<string> CallAsync(
        string action,
        int orgId,
        int appId,
        int? employeeId = null,
        string? search = null,
        string? status = null,
        int? limit = null,
        int? offset = null,
        int? fiscalYearId = null,
        string? fullName = null,
        string? email = null,
        string? phone = null,
        string? gender = null,
        DateOnly? dateOfBirth = null,
        DateOnly? joinDate = null,
        string? employmentType = null,
        int? branchId = null,
        int? departmentId = null,
        int? staffRoleId = null,
        string? address = null,
        string? productIds = null,
        string? scheduleJson = null) =>
        _db.ExecuteJsonFunctionAsync(
            Fn,
            Varchar(action),
            Int(orgId),
            Int(appId),
            NullableInt(employeeId),
            Varchar(search),
            Varchar(status),
            NullableInt(limit),
            NullableInt(offset),
            NullableInt(fiscalYearId),
            Varchar(fullName),
            Varchar(email),
            Varchar(phone),
            Varchar(gender),
            Date(dateOfBirth),
            Date(joinDate),
            Varchar(employmentType),
            NullableInt(branchId),
            NullableInt(departmentId),
            NullableInt(staffRoleId),
            Varchar(address),
            Varchar(productIds),
            Jsonb(scheduleJson));

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

    private static NpgsqlParameter Jsonb(string? value) =>
        new()
        {
            Value = string.IsNullOrWhiteSpace(value) ? DBNull.Value : value,
            NpgsqlDbType = NpgsqlDbType.Jsonb
        };

    private static NpgsqlParameter Date(DateOnly? value) =>
        new()
        {
            Value = value.HasValue ? value.Value : DBNull.Value,
            NpgsqlDbType = NpgsqlDbType.Date
        };
}
