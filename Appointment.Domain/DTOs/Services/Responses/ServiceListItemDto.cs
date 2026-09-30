namespace Appointment.Domain.DTOs.Services.Responses;

public sealed class ServiceListItemDto
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
    public int? TaxGroupId { get; set; }
    public string? TaxGroupName { get; set; }
    public decimal? CgstPer { get; set; }
    public decimal? SgstPer { get; set; }
    public decimal? IgstPer { get; set; }
    public string? GstMode { get; set; }
    public bool IsEmployeeRequired { get; set; } = true;
    public bool IsResourceRequired { get; set; }
    public long TotalCount { get; set; }
}
