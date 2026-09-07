namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalScheduleDto
{
    public int EmployeeId { get; set; }
    public int BranchId { get; set; }
    public string? Timezone { get; set; }
    public int ConsultDurationMinutes { get; set; }
    public int BufferMinutes { get; set; }
    public IReadOnlyList<ProfessionalScheduleDayDto> Days { get; set; } = [];
}

public sealed class ProfessionalScheduleDayDto
{
    public int DayOfWeek { get; set; }
    public string? DayName { get; set; }
    public bool IsClosed { get; set; }
    public string? Start { get; set; }
    public string? End { get; set; }
    public string? BreakStart { get; set; }
    public string? BreakEnd { get; set; }
    public Guid? ShiftId { get; set; }
    public string? ShiftName { get; set; }
}
