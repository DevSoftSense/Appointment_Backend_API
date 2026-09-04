namespace Appointment.Domain.DTOs.Services.Responses;

public sealed class ServiceDetailDto
{
    public int ProductId { get; set; }
    public string? ShortName { get; set; }
    public string? ProductName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal? SellingPrice { get; set; }
    public int? DurationMinutes { get; set; }
    public string? SalesDescription { get; set; }
    public string? ProductType { get; set; }
    public bool IsActive { get; set; }
    public int? FiscalYearId { get; set; }
}
