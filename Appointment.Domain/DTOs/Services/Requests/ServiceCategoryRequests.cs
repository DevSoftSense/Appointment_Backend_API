namespace Appointment.Domain.DTOs.Services.Requests;

public sealed class SaveServiceCategoryRequest
{
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateServiceCategoryRequest
{
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
}
