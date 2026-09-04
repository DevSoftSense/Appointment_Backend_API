namespace Appointment.Domain.DTOs.Services.Responses;

public sealed class CreateServiceResponse
{
    public int ProductId { get; set; }
    public string? ShortName { get; set; }
    public string? ProductName { get; set; }
    public bool IsActive { get; set; }
}
