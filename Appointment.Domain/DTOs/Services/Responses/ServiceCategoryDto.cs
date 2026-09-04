namespace Appointment.Domain.DTOs.Services.Responses;

public sealed class ServiceCategoryDto
{
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
}
