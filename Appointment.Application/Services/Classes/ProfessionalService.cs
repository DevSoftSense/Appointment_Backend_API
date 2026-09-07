using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;
using Appointment.Domain.Exceptions;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

public sealed class ProfessionalService : IProfessionalService
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ProfessionalService> _logger;

    public ProfessionalService(
        IProfessionalRepository professionalRepository,
        IConfiguration configuration,
        ILogger<ProfessionalService> logger)
    {
        _professionalRepository = professionalRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ProfessionalListResponse> GetProfessionalsAsync(
        int orgId,
        GetProfessionalsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetProfessionalsRequest();
        if (request.Limit <= 0) request.Limit = 50;
        if (request.Offset < 0) request.Offset = 0;

        var appId = GetAppId();
        var items = await _professionalRepository.GetProfessionalsAsync(orgId, appId, request, cancellationToken);

        return new ProfessionalListResponse
        {
            Items = items,
            TotalCount = items.Count > 0 ? items[0].TotalCount : 0
        };
    }

    public async Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));

        var appId = GetAppId();
        return await _professionalRepository.GetProfessionalByIdAsync(orgId, appId, employeeId, cancellationToken);
    }

    public async Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        CreateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new ArgumentException("Email is not valid.");

        var appId = GetAppId();

        try
        {
            return await _professionalRepository.CreateProfessionalAsync(orgId, appId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate professional email for orgId {OrgId}", orgId);
            throw new ProfessionalDuplicateEmailException(
                "Professional with this email already exists for this organisation and app");
        }
    }

    public async Task<CreateProfessionalResponse> UpdateProfessionalAsync(
        int orgId,
        int employeeId,
        UpdateProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.FullName))
            throw new ArgumentException("Full name is required.");

        if (!string.IsNullOrWhiteSpace(request.Email) && !request.Email.Contains('@'))
            throw new ArgumentException("Email is not valid.");

        var appId = GetAppId();

        try
        {
            return await _professionalRepository.UpdateProfessionalAsync(orgId, appId, employeeId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate professional email on update for orgId {OrgId}", orgId);
            throw new ProfessionalDuplicateEmailException(
                "Professional with this email already exists for this organisation and app");
        }
    }

    public async Task<CreateProfessionalResponse> DeactivateProfessionalAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));

        var appId = GetAppId();
        return await _professionalRepository.DeactivateProfessionalAsync(orgId, appId, employeeId, cancellationToken);
    }

    public async Task<ProfessionalStatsDto> GetProfessionalStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _professionalRepository.GetProfessionalStatsAsync(orgId, appId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> ListProfessionalServicesAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));

        var appId = GetAppId();
        return await _professionalRepository.ListProfessionalServicesAsync(orgId, appId, employeeId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> GetAvailableServicesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _professionalRepository.GetAvailableServicesAsync(orgId, appId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfessionalServiceItemDto>> SetProfessionalServicesAsync(
        int orgId,
        int employeeId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));

        var appId = GetAppId();
        return await _professionalRepository.SetProfessionalServicesAsync(
            orgId, appId, employeeId, productIds ?? [], cancellationToken);
    }

    public async Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));

        var appId = GetAppId();
        return await _professionalRepository.GetScheduleAsync(orgId, appId, employeeId, cancellationToken);
    }

    public async Task<ProfessionalScheduleDto> SaveScheduleAsync(
        int orgId,
        int employeeId,
        SaveProfessionalScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (employeeId <= 0)
            throw new ArgumentException("Professional id is required.", nameof(employeeId));
        ArgumentNullException.ThrowIfNull(request);

        if (request.ConsultDurationMinutes <= 0)
            throw new ArgumentException("Consult duration must be greater than 0.", nameof(request));
        if (request.BufferMinutes < 0)
            throw new ArgumentException("Buffer minutes cannot be negative.", nameof(request));

        var appId = GetAppId();
        return await _professionalRepository.SaveScheduleAsync(orgId, appId, employeeId, request, cancellationToken);
    }

    public async Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        DateOnly fromDate,
        DateOnly toDate,
        IEnumerable<int>? employeeIds = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (toDate < fromDate)
            throw new ArgumentException("toDate must be on or after fromDate.");
        if (toDate.DayNumber - fromDate.DayNumber > 62)
            throw new ArgumentException("Date range cannot exceed 62 days.");

        var ids = (employeeIds ?? []).Where(id => id > 0).Distinct().ToList();
        if (ids.Count == 0)
            throw new ArgumentException("At least one employee id is required.");

        var appId = GetAppId();
        return await _professionalRepository.GetScheduleGridAsync(orgId, appId, fromDate, toDate, ids, cancellationToken);
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _professionalRepository.GetBranchesAsync(orgId, appId, cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _professionalRepository.GetDepartmentsAsync(orgId, appId, branchId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var appId = GetAppId();
        return await _professionalRepository.GetRolesAsync(orgId, appId, branchId, cancellationToken);
    }

    /// <summary>
    /// SOC app id for Appointment (public.app_id). Prefers Appointment:AppId; falls back to ProductId.
    /// </summary>
    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId")
                    ?? 0;
        if (appId <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
