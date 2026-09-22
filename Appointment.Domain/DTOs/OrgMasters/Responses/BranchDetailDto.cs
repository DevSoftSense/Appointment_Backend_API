namespace Appointment.Domain.DTOs.OrgMasters.Responses;

public sealed class BranchDetailDto
{
    public int BranchId { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
    public string? BranchName { get; set; }
    public string? BranchCode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Timezone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Pincode { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? UpdatedOn { get; set; }
    public long TotalCount { get; set; }
}
