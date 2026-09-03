namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class CreateProfessionalResponse
{
    public int EmployeeId { get; set; }
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? Status { get; set; }
}
