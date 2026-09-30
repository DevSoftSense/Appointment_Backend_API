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

    public Task<MyPermissionsDto> GetMyAsync(
        int orgId, IReadOnlyList<string> roleCodes, bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var codes = (roleCodes ?? [])
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Select(c => c.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!isAdmin)
            isAdmin = IsAdminRoleCodes(codes);

        return _repository.GetMyAsync(orgId, GetAppId(), codes, isAdmin, cancellationToken);
    }

    public Task SeedPresetsAsync(int orgId, CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        return _repository.SeedPresetsAsync(orgId, GetAppId(), cancellationToken);
    }

    public bool IsAdminRoleCodes(IEnumerable<string>? roleCodes) =>
        (roleCodes ?? []).Any(c =>
        {
            var code = (c ?? "").Trim().ToUpperInvariant();
            return code.EndsWith("_ADMIN", StringComparison.Ordinal)
                   || code is "APPOINTMENT_ADMIN" or "ADMIN";
        });

    public async Task<bool> CanAsync(
        int orgId,
        IReadOnlyList<string> roleCodes,
        string menuCode,
        string action,
        CancellationToken cancellationToken = default)
    {
        ValidateOrg(orgId);
        var code = (menuCode ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var my = await GetMyAsync(orgId, roleCodes ?? [], false, cancellationToken);
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
