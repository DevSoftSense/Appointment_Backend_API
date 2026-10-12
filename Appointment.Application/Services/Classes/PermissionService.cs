using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Permissions.Requests;
using Appointment.Domain.DTOs.Permissions.Responses;
using Appointment.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Appointment.Application.Services.Classes;

public sealed class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _repository;
    private readonly IConfiguration _configuration;

    public PermissionService(IPermissionRepository repository, IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    public Task<IReadOnlyList<PermissionModuleDto>> ListModulesAsync(
        int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _repository.ListModulesAsync(orgId, GetAppId(), cancellationToken);
    }

    public Task<IReadOnlyList<PermissionModuleDto>> GetRoleAsync(
        int orgId, string roleCode, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code is required.", nameof(roleCode));
        return _repository.GetRoleAsync(orgId, GetAppId(), roleCode.Trim(), cancellationToken);
    }

    public Task<IReadOnlyList<PermissionModuleDto>> SaveRoleAsync(
        int orgId, string roleCode, SaveRolePermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new ArgumentException("Role code is required.", nameof(roleCode));
        request ??= new SaveRolePermissionsRequest();
        return _repository.SaveRoleAsync(orgId, GetAppId(), roleCode.Trim(), request, cancellationToken);
    }

    public async Task<MyPermissionsDto> GetMyAsync(
        int orgId, IReadOnlyList<string> roleCodes, bool isAdmin, string? userType = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var codes = (roleCodes ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Full ACL bypass = org owner only (product *_ADMIN uses the matrix)
        if (!isAdmin)
            isAdmin = IsOrgOwner(codes, userType);

        var my = await _repository.GetMyAsync(orgId, GetAppId(), codes, isAdmin, cancellationToken);

        // Product Admin: keep Owner-selected menus from matrix; always allow Users & Roles
        // so Admin can Save permissions like Owner (without seeing every other menu by default).
        if (!my.IsAdmin
            && !my.FailOpen
            && !IsOrgOwner(codes, userType)
            && IsAdminRoleCodes(codes))
        {
            my = EnsureProductAdminManagementAccess(my);
        }

        return my;
    }

    public Task SeedPresetsAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _repository.SeedPresetsAsync(orgId, GetAppId(), cancellationToken);
    }

    public bool IsAdminRoleCodes(IEnumerable<string>? roleCodes) =>
        (roleCodes ?? []).Any(IsAdminRoleCode);

    /// <summary>
    /// SoftOnCloud org-owner role codes (full ACL bypass + can edit permission matrix).
    /// </summary>
    public static bool IsOwnerRoleCode(string? roleCode)
    {
        var code = (roleCode ?? "").Trim().ToUpperInvariant();
        return code is "ORG_OWNER" or "OWNER" or "ORGADMIN";
    }

    /// <summary>
    /// SoftOnCloud product admin role (e.g. APPOINTMENT_ADMIN).
    /// Can manage Users / Roles / Permissions Save; sidebar still follows the matrix
    /// (does <b>not</b> get full ACL bypass — that is Owner only).
    /// </summary>
    public static bool IsAdminRoleCode(string? roleCode)
    {
        var code = (roleCode ?? "").Trim().ToUpperInvariant();
        if (code.Length == 0 || IsOwnerRoleCode(code)) return false;
        if (code.EndsWith("_ADMIN", StringComparison.Ordinal)) return true;
        return code is "APPOINTMENT_ADMIN" or "ADMIN";
    }

    public bool IsPrimeUserType(string? userType)
    {
        var t = (userType ?? "").Trim().ToLowerInvariant();
        return t is "prime" or "owner" or "org_owner";
    }

    /// <summary>
    /// Organisation owner only (ORG_OWNER / OWNER / ORGADMIN or userType prime).
    /// Full sidebar bypass. Product <c>*_ADMIN</c> is not owner.
    /// </summary>
    public bool IsOrgOwner(IEnumerable<string>? roleCodes, string? userType) =>
        (roleCodes ?? []).Any(IsOwnerRoleCode) || IsPrimeUserType(userType);

    /// <summary>
    /// Who may manage Users / Roles / Permissions (Save / Seed / Invite).
    /// Owner <b>or</b> product Admin (<c>*_ADMIN</c>). ACL bypass remains Owner-only via GetMyAsync.
    /// </summary>
    public bool IsAppAdmin(IEnumerable<string>? roleCodes, string? userType) =>
        IsOrgOwner(roleCodes, userType) || IsAdminRoleCodes(roleCodes);

    public async Task<bool> CanAsync(
        int orgId,
        IReadOnlyList<string> roleCodes,
        string menuCode,
        string action,
        string? userType = null,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var code = (menuCode ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var my = await GetMyAsync(orgId, roleCodes ?? [], false, userType, cancellationToken);
        if (my.IsAdmin || my.FailOpen)
            return true;

        var module = (my.Modules ?? [])
            .FirstOrDefault(m => MenuCodeMatches(m.MenuCode, code));
        if (module is null)
            return false;

        return (action ?? "").Trim().ToLowerInvariant() switch
        {
            "view" => module.CanView,
            "add" => module.CanAdd,
            "edit" => module.CanEdit,
            "delete" => module.CanDelete,
            _ => false
        };
    }

    /// <summary>
    /// Menus product Admin always gets so they can open Settings → Users &amp; Roles
    /// and Save the matrix (same capability as Owner for permission management).
    /// </summary>
    private static readonly string[] ProductAdminManagementMenuCodes =
    [
        "APPOINTMENT_SETTINGS",
        "APPOINTMENT_SETTINGS_USERS_ROLES",
        "SETTINGS",
        "SETTINGS_USERS_ROLES",
    ];

    private static MyPermissionsDto EnsureProductAdminManagementAccess(MyPermissionsDto my)
    {
        var list = (my.Modules ?? []).ToList();
        foreach (var code in ProductAdminManagementMenuCodes)
        {
            var existing = list.FirstOrDefault(m => MenuCodeMatches(m.MenuCode, code));
            if (existing is not null)
            {
                existing.CanView = true;
                existing.CanAdd = true;
                existing.CanEdit = true;
                existing.CanDelete = true;
                continue;
            }

            list.Add(new PermissionModuleDto
            {
                MenuCode = code,
                MenuName = code.Contains("USERS_ROLES", StringComparison.OrdinalIgnoreCase)
                    ? "Users & Roles"
                    : "Settings",
                CanView = true,
                CanAdd = true,
                CanEdit = true,
                CanDelete = true,
            });
        }

        my.Modules = list;
        return my;
    }

    /// <summary>Match CUSTOMERS ↔ APPOINTMENT_CUSTOMERS (live SoftOnCloud prefix).</summary>
    private static bool MenuCodeMatches(string? stored, string requested)
    {
        var a = (stored ?? "").Trim().ToUpperInvariant();
        var b = (requested ?? "").Trim().ToUpperInvariant();
        if (a.Length == 0 || b.Length == 0) return false;
        if (string.Equals(a, b, StringComparison.Ordinal)) return true;
        if (a.StartsWith("APPOINTMENT_", StringComparison.Ordinal) &&
            string.Equals(a["APPOINTMENT_".Length..], b, StringComparison.Ordinal))
            return true;
        if (b.StartsWith("APPOINTMENT_", StringComparison.Ordinal) &&
            string.Equals(b["APPOINTMENT_".Length..], a, StringComparison.Ordinal))
            return true;
        return false;
    }

    private static void ValidateOrg(int orgId)
    {
        if (orgId <= 0)
            throw new ArgumentException("Organisation id is required.", nameof(orgId));
    }

    private int GetAppId()
    {
        var appId = _configuration.GetValue<int?>("Appointment:AppId")
                    ?? _configuration.GetValue<int?>("Appointment:ProductId");
        if (appId is null or <= 0)
            throw new InvalidOperationException("Appointment:AppId (or ProductId) is not configured.");
        return appId.Value;
    }
}
