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

    Task<ProfessionalStatsDto> GetProfessionalStatsAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> ListProfessionalServicesAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> GetAvailableServicesAsync(
        int orgId,
        int appId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> SetProfessionalServicesAsync(
        int orgId,
        int appId,
        int employeeId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int appId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleDto> SaveScheduleAsync(
        int orgId,
        int appId,
        int employeeId,
        SaveProfessionalScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        int appId,
        DateOnly fromDate,
        DateOnly toDate,
        IEnumerable<int> employeeIds,
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
