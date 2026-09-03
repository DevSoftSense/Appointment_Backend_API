namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalListItemDto
{
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int? StaffRoleId { get; set; }
    public string? RoleName { get; set; }
    public string? EmploymentType { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public long TotalCount { get; set; }
}
