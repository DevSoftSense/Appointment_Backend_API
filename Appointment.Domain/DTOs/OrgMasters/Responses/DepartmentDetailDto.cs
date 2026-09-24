namespace Appointment.Domain.DTOs.OrgMasters.Responses;

public sealed class DepartmentDetailDto
{
    public int DepartmentId { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public long TotalCount { get; set; }
}
