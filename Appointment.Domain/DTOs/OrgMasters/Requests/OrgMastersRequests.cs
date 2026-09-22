namespace Appointment.Domain.DTOs.OrgMasters.Requests;

public sealed class GetBranchesRequest
{
    public bool IncludeInactive { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; }
}

public sealed class SaveBranchRequest
{
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
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateBranchRequest
{
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
    public bool? IsActive { get; set; }
}
