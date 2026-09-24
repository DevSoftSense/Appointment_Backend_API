using Appointment.Domain.DTOs.OrgMasters.Requests;
using Appointment.Domain.DTOs.OrgMasters.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IOrgMastersService
{
    Task<IReadOnlyList<BranchDetailDto>> ListBranchesAsync(
        int orgId, GetBranchesRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> GetBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> CreateBranchAsync(
        int orgId, SaveBranchRequest request, CancellationToken cancellationToken = default);

    Task<BranchDetailDto> UpdateBranchAsync(
        int orgId, int branchId, UpdateBranchRequest request, CancellationToken cancellationToken = default);

    Task DeactivateBranchAsync(
        int orgId, int branchId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentDetailDto>> ListDepartmentsAsync(
        int orgId, GetDepartmentsRequest request, CancellationToken cancellationToken = default);

    Task<DepartmentDetailDto> GetDepartmentAsync(
        int orgId, int departmentId, CancellationToken cancellationToken = default);

    Task<DepartmentDetailDto> CreateDepartmentAsync(
        int orgId, SaveDepartmentRequest request, CancellationToken cancellationToken = default);

    Task<DepartmentDetailDto> UpdateDepartmentAsync(
        int orgId, int departmentId, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);

    Task DeactivateDepartmentAsync(
        int orgId, int departmentId, CancellationToken cancellationToken = default);
}
