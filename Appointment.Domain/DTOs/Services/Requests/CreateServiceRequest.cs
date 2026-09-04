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
}
