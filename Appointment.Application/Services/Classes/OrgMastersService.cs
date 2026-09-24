using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.OrgMasters.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Appointment.Application.Services.Classes;

public sealed class OrgMastersService : IOrgMastersService
{
    private readonly IOrgMastersRepository _orgMastersRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrgMastersService> _logger;

    public OrgMastersService(
        IOrgMastersRepository orgMastersRepository,
        IConfiguration configuration,
        ILogger<OrgMastersService> logger)
    {
        _orgMastersRepository = orgMastersRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public Task<IReadOnlyList<BranchDetailDto>> ListBranchesAsync(
        int orgId, GetBranchesRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetBranchesRequest();
        if (request.Limit <= 0) request.Limit = 100;
        if (request.Offset < 0) request.Offset = 0;
        return _orgMastersRepository.ListBranchesAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<BranchDetailDto> GetBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (branchId <= 0) throw new ArgumentException("Branch id is required.");
        return _orgMastersRepository.GetBranchAsync(orgId, GetAppId(), branchId, cancellationToken);
    }

    public Task<BranchDetailDto> CreateBranchAsync(
        int orgId, SaveBranchRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.BranchName))
            throw new ArgumentException("Branch name is required.");

        _logger.LogInformation("Creating branch for org {OrgId}", orgId);
        return _orgMastersRepository.CreateBranchAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<BranchDetailDto> UpdateBranchAsync(
        int orgId, int branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (branchId <= 0) throw new ArgumentException("Branch id is required.");
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.BranchName))
            throw new ArgumentException("Branch name is required.");
        if (string.IsNullOrWhiteSpace(request.BranchCode))
            throw new ArgumentException("Branch code is required.");

        return _orgMastersRepository.UpdateBranchAsync(orgId, GetAppId(), branchId, request, cancellationToken);
    }

    public Task DeactivateBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (branchId <= 0) throw new ArgumentException("Branch id is required.");
        return _orgMastersRepository.DeactivateBranchAsync(orgId, GetAppId(), branchId, cancellationToken);
    }

    public Task<IReadOnlyList<DepartmentDetailDto>> ListDepartmentsAsync(
        int orgId, GetDepartmentsRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        request ??= new GetDepartmentsRequest();
        if (request.Limit <= 0) request.Limit = 100;
        if (request.Offset < 0) request.Offset = 0;
        return _orgMastersRepository.ListDepartmentsAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<DepartmentDetailDto> GetDepartmentAsync(
        int orgId, int departmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (departmentId <= 0) throw new ArgumentException("Department id is required.");
        return _orgMastersRepository.GetDepartmentAsync(orgId, GetAppId(), departmentId, cancellationToken);
    }

    public Task<DepartmentDetailDto> CreateDepartmentAsync(
        int orgId, SaveDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.BranchId <= 0)
            throw new ArgumentException("Branch is required.");
        if (string.IsNullOrWhiteSpace(request.DepartmentName))
            throw new ArgumentException("Department name is required.");

        _logger.LogInformation("Creating department for org {OrgId}", orgId);
        return _orgMastersRepository.CreateDepartmentAsync(orgId, GetAppId(), request, cancellationToken);
    }

    public Task<DepartmentDetailDto> UpdateDepartmentAsync(
        int orgId, int departmentId, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (departmentId <= 0) throw new ArgumentException("Department id is required.");
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.DepartmentName))
            throw new ArgumentException("Department name is required.");

        return _orgMastersRepository.UpdateDepartmentAsync(orgId, GetAppId(), departmentId, request, cancellationToken);
    }

    public Task DeactivateDepartmentAsync(
        int orgId, int departmentId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (departmentId <= 0) throw new ArgumentException("Department id is required.");
        return _orgMastersRepository.DeactivateDepartmentAsync(orgId, GetAppId(), departmentId, cancellationToken);
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }

    private int GetAppId()
    {
        return _configuration.GetValue<int?>("Appointment:AppId")
               ?? _configuration.GetValue<int?>("Appointment:ProductId")
               ?? 25;
    }
}
