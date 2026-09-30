namespace Appointment.Domain.DTOs.Permissions.Requests;

public sealed class SaveRolePermissionsRequest
{
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
