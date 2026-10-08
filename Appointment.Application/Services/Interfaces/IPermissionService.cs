using Appointment.Domain.DTOs.Permissions.Requests;
using Appointment.Domain.DTOs.Permissions.Responses;

namespace Appointment.Application.Services.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionModuleDto>> ListModulesAsync(
        int orgId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionModuleDto>> GetRoleAsync(
        int orgId, string roleCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PermissionModuleDto>> SaveRoleAsync(
        int orgId, string roleCode, SaveRolePermissionsRequest request,
        CancellationToken cancellationToken = default);

    Task<MyPermissionsDto> GetMyAsync(
        int orgId, IReadOnlyList<string> roleCodes, bool isAdmin, string? userType = null,
        CancellationToken cancellationToken = default);

    Task SeedPresetsAsync(int orgId, CancellationToken cancellationToken = default);

    bool IsAdminRoleCodes(IEnumerable<string>? roleCodes);

    bool IsPrimeUserType(string? userType);

    bool IsAppAdmin(IEnumerable<string>? roleCodes, string? userType);

    /// <summary>
    /// True when admin, fail-open (no matrix rows yet), or the role has the flag on menuCode.
    /// action: view | add | edit | delete
    /// </summary>
    Task<bool> CanAsync(
        int orgId,
        IReadOnlyList<string> roleCodes,
        string menuCode,
        string action,
        string? userType = null,
        CancellationToken cancellationToken = default);
}
