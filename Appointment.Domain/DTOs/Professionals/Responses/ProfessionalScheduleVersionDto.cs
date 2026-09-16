namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalScheduleVersionDto
{
    public DateOnly? EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public int DayCount { get; set; }
    public bool IsCurrent { get; set; }
}
