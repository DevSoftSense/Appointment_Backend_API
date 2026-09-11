namespace Appointment.Domain.DTOs.Cabins.Responses;

public sealed class CabinListItemDto
{
    public long CabinResourceId { get; set; }
    public long? OrgId { get; set; }
    public string? ResourceCode { get; set; }
    public string? ResourceName { get; set; }
    public string? ResourceType { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? FiscalYearId { get; set; }
    public long TotalCount { get; set; }
    public long BookedToday { get; set; }
}
