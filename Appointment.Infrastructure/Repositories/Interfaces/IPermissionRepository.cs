using Appointment.Domain.DTOs.Permissions.Requests;
using Appointment.Domain.DTOs.Permissions.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyList<PermissionModuleDto>> ListModulesAsync(
        int orgId, int appId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionModuleDto>> GetRoleAsync(
        int orgId, int appId, string roleCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionModuleDto>> SaveRoleAsync(
        int orgId, int appId, string roleCode, SaveRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task<MyPermissionsDto> GetMyAsync(
        int orgId, int appId, IReadOnlyList<string> roleCodes, bool isAdmin,
        CancellationToken cancellationToken = default);

    Task SeedPresetsAsync(int orgId, int appId, CancellationToken cancellationToken = default);
}
