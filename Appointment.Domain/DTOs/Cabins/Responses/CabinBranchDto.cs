namespace Appointment.Domain.DTOs.Cabins.Responses;

public sealed class CabinBranchDto
{
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; }
}
