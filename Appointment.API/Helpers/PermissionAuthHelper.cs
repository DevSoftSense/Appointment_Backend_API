using Appointment.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Appointment.API.Helpers;

/// <summary>
/// Thin ACL helper for write endpoints. Role codes come from X-Role-Codes (or roleCodes query),
/// matching SoftOnCloud productContext.roles on the client.
/// </summary>
public static class PermissionAuthHelper
{
    public const string RoleCodesHeader = "X-Role-Codes";

    public static IReadOnlyList<string> GetRoleCodes(HttpRequest request)
    {
        var header = request.Headers[RoleCodesHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(header))
            return SplitCodes(header);

        if (request.Query.TryGetValue("roleCodes", out var q) && !string.IsNullOrWhiteSpace(q))
            return SplitCodes(q.ToString());

        return [];
    }

    public static async Task<IActionResult?> ForbidUnlessCanAsync(
        ControllerBase controller,
        IPermissionService permissionService,
        int orgId,
        string menuCode,
        string action,
        CancellationToken cancellationToken = default)
    {
        var codes = GetRoleCodes(controller.Request);
        var allowed = await permissionService.CanAsync(
            orgId, codes, menuCode, action, cancellationToken);
        if (allowed) return null;

        return controller.StatusCode(
            StatusCodes.Status403Forbidden,
            new { message = "You do not have permission for this action." });
    }

    private static List<string> SplitCodes(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? []
            : raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
}
