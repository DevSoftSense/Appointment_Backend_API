namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ScheduleConflictDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset? StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public string? Status { get; set; }
    public string? CustomerName { get; set; }
    /// <summary>day_closed | outside_hours | overlaps_break</summary>
    public string? Reason { get; set; }
}
