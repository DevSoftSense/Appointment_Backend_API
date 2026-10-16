namespace Appointment.Domain.DTOs.Permissions.Requests;

public sealed class SaveRolePermissionsRequest
{
    /// <summary>SoftOnCloud numeric role id (from Roles API). Stored on tab_role_permission.role_id.</summary>
    public int? RoleId { get; set; }

    public IReadOnlyList<RolePermissionItemRequest> Items { get; set; } = [];
}

public sealed class RolePermissionItemRequest
{
    public string? MenuCode { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
