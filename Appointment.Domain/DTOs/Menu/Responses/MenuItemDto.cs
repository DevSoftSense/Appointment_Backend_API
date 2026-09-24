namespace Appointment.Domain.DTOs.Menu.Responses;

public sealed class MenuItemDto
{
    public int MenuId { get; set; }
    public int? ParentMenuId { get; set; }
    public string? MenuCode { get; set; }
    public string? MenuName { get; set; }
    public string? MenuUrl { get; set; }
    public string? IconName { get; set; }
    public int SortOrder { get; set; }
    public List<MenuItemDto> Children { get; set; } = [];
}
