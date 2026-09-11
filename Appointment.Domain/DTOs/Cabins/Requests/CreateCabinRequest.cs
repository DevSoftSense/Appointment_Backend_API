namespace Appointment.Domain.DTOs.Cabins.Requests;

public sealed class CreateCabinRequest
{
    public string? ResourceName { get; set; }
    public string? ResourceCode { get; set; }
    public string? ResourceType { get; set; }
    public string? Description { get; set; }
    public int? BranchId { get; set; }
    public bool? IsActive { get; set; }
    public int? FiscalYearId { get; set; }
}
