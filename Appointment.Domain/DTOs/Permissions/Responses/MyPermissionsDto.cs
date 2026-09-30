namespace Appointment.Domain.DTOs.Permissions.Responses;

public sealed class MyPermissionsDto
{
    public bool IsAdmin { get; set; }
    public bool FailOpen { get; set; }
    public IReadOnlyList<PermissionModuleDto> Modules { get; set; } = [];
}
