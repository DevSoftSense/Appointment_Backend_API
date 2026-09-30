namespace Appointment.Domain.DTOs.Services.Requests;

public sealed class CreateServiceRequest
{
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public decimal? SellingPrice { get; set; }
    public int? DurationMinutes { get; set; }
    public string? SalesDescription { get; set; }
    public bool? IsActive { get; set; }
    public int? FiscalYearId { get; set; }

    /// <summary>Optional SoftOnCloud tax group. Prefer CgstPer/SgstPer/IgstPer to find-or-create.</summary>
    public int? TaxGroupId { get; set; }

    public decimal? CgstPer { get; set; }
    public decimal? SgstPer { get; set; }
    public decimal? IgstPer { get; set; }

    /// <summary>Default true — consultation-style services need an employee (Professional).</summary>
    public bool? IsEmployeeRequired { get; set; }

    /// <summary>When true, booking must pick a resource (Cabin / machine).</summary>
    public bool? IsResourceRequired { get; set; }
}
