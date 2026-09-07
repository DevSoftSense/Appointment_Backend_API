namespace Appointment.Domain.DTOs.Professionals.Requests;

public sealed class SaveProfessionalScheduleRequest
{
    public IReadOnlyList<SaveProfessionalScheduleDayRequest> Days { get; set; } = [];
    public int ConsultDurationMinutes { get; set; } = 30;
    public int BufferMinutes { get; set; } = 15;
    public string? Timezone { get; set; }
    public DateOnly? EffectiveFrom { get; set; }
}

public sealed class SaveProfessionalScheduleDayRequest
{
    /// <summary>1 = Monday … 7 = Sunday</summary>
    public int DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public string? BreakStart { get; set; }
    public string? BreakEnd { get; set; }
}
