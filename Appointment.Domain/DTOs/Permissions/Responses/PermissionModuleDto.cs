namespace Appointment.Domain.DTOs.Permissions.Responses;

public sealed class PermissionModuleDto
{
    public int MenuId { get; set; }
    public int? ParentMenuId { get; set; }
    public string? MenuCode { get; set; }
    public string? MenuName { get; set; }
    public string? MenuUrl { get; set; }
    public string? IconName { get; set; }
    public int SortOrder { get; set; }
    public int Depth { get; set; }
    public bool CanView { get; set; }
    public bool CanAdd { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
