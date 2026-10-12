namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class BranchDto
{
    public int BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    /// <summary>City / location for FilterBar Location dropdown (from tab_branch_master.city).</summary>
    public string? City { get; set; }
}
