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

        var productId = GetProductId();
        var items = await _professionalRepository.GetProfessionalsAsync(orgId, productId, request, cancellationToken);

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

        var productId = GetProductId();
        return await _professionalRepository.GetProfessionalByIdAsync(orgId, productId, employeeId, cancellationToken);
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

        var productId = GetProductId();

        try
        {
            return await _professionalRepository.CreateProfessionalAsync(orgId, productId, request, cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            _logger.LogInformation(ex, "Duplicate professional email for orgId {OrgId}", orgId);
            throw new ProfessionalDuplicateEmailException(
                "Professional with this email already exists for this organisation and product");
        }
    }

    public async Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var productId = GetProductId();
        return await _professionalRepository.GetBranchesAsync(orgId, productId, cancellationToken);
    }

    public async Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var productId = GetProductId();
        return await _professionalRepository.GetDepartmentsAsync(orgId, productId, branchId, cancellationToken);
    }

    public async Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var productId = GetProductId();
        return await _professionalRepository.GetRolesAsync(orgId, productId, branchId, cancellationToken);
    }

    private int GetProductId()
    {
        var productId = _configuration.GetValue<int?>("Appointment:ProductId") ?? 0;
        if (productId <= 0)
            throw new InvalidOperationException("Appointment:ProductId is not configured.");
        return productId;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation ID is required.");
    }
}
