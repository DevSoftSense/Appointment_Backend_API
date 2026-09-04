namespace Appointment.Domain.DTOs.Services.Requests;

public sealed class GetServicesRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
