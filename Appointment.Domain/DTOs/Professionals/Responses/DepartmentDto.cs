namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public int BranchId { get; set; }
}
