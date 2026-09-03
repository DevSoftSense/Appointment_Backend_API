namespace Appointment.Domain.DTOs.Professionals.Requests;

public sealed class CreateProfessionalRequest
{
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? JoinDate { get; set; }
    public string? EmploymentType { get; set; }
    public int? BranchId { get; set; }
    public int? DepartmentId { get; set; }
    public int? StaffRoleId { get; set; }
    public string? Address { get; set; }
    public string? Status { get; set; }
    public int? FiscalYearId { get; set; }
}
