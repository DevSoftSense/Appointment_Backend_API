using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IProfessionalRepository
{
    Task<IReadOnlyList<ProfessionalListItemDto>> GetProfessionalsAsync(
        int orgId,
        int appId,
        GetProfessionalsRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        int appId,
        CreateProfessionalRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> UpdateProfessionalAsync(
        int orgId,
        int appId,
        int employeeId,
        UpdateProfessionalRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> DeactivateProfessionalAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int appId,
        int? branchId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int appId,
        int? branchId,
        CancellationToken cancellationToken = default);
}
