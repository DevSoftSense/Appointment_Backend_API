namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalServiceItemDto
{
    public long? MappingId { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ShortName { get; set; }
    public bool IsActive { get; set; } = true;
}
