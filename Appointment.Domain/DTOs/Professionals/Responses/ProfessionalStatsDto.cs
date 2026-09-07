namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalStatsDto
{
    public long Total { get; set; }
    public long Active { get; set; }
    public long Inactive { get; set; }
}
