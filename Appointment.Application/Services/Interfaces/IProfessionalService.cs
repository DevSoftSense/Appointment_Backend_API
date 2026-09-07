using Appointment.Domain.DTOs.Professionals.Requests;
using Appointment.Domain.DTOs.Professionals.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IProfessionalService
{
    Task<ProfessionalListResponse> GetProfessionalsAsync(
        int orgId,
        GetProfessionalsRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalDetailDto?> GetProfessionalByIdAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> CreateProfessionalAsync(
        int orgId,
        CreateProfessionalRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> UpdateProfessionalAsync(
        int orgId,
        int employeeId,
        UpdateProfessionalRequest request,
        CancellationToken cancellationToken = default);

    Task<CreateProfessionalResponse> DeactivateProfessionalAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<ProfessionalStatsDto> GetProfessionalStatsAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> ListProfessionalServicesAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> GetAvailableServicesAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalServiceItemDto>> SetProfessionalServicesAsync(
        int orgId,
        int employeeId,
        IEnumerable<int> productIds,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleDto> GetScheduleAsync(
        int orgId,
        int employeeId,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleDto> SaveScheduleAsync(
        int orgId,
        int employeeId,
        SaveProfessionalScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<ProfessionalScheduleGridDto> GetScheduleGridAsync(
        int orgId,
        DateOnly fromDate,
        DateOnly toDate,
        IEnumerable<int>? employeeIds = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BranchDto>> GetBranchesAsync(
        int orgId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DepartmentDto>> GetDepartmentsAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ProfessionalRoleDto>> GetRolesAsync(
        int orgId,
        int? branchId,
        CancellationToken cancellationToken = default);
}
