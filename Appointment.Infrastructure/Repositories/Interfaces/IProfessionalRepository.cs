using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IProfessionalRepository
{
    Task<IReadOnlyList<ProfessionalListItemDto>> GetProfessionalsAsync(
        int orgId,
        int productId,
        GetProfessionalsRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int productId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        int productId,
        CreateProfessionalRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int productId,
        int? branchId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int productId,
        int? branchId,
        CancellationToken cancellationToken = default);
}
