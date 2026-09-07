namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalScheduleGridDto
{
    public string? FromDate { get; set; }
    public string? ToDate { get; set; }
    public string? Timezone { get; set; }
    public IReadOnlyList<ProfessionalScheduleGridProfessionalDto> Professionals { get; set; } = [];
}

public sealed class ProfessionalScheduleGridProfessionalDto
{
    public int EmployeeId { get; set; }
    public string? FullName { get; set; }
    public int BranchId { get; set; }
    public IReadOnlyList<ProfessionalScheduleGridDayDto> Days { get; set; } = [];
}

public sealed class ProfessionalScheduleGridDayDto
{
    public string? Date { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsClosed { get; set; }
    public string? ShiftStart { get; set; }
    public string? ShiftEnd { get; set; }
    public IReadOnlyList<ProfessionalScheduleGridSlotDto> Slots { get; set; } = [];
}

public sealed class ProfessionalScheduleGridSlotDto
{
    public string? Start { get; set; }
    public string? End { get; set; }
    /// <summary>available | break | booked | walk_in | blocked</summary>
    public string? Status { get; set; }
    public long? AppointmentId { get; set; }
    public string? CustomerName { get; set; }
    public string? Source { get; set; }
    public string? Type { get; set; }
    public string? Details { get; set; }
}
