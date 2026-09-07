namespace Appointment.Domain.DTOs.Appointments.Responses;

public sealed class AppointmentStatsDto
{
    public long Total { get; set; }
    public long Upcoming { get; set; }
    public long Completed { get; set; }
    public long Cancelled { get; set; }
    public long NoShow { get; set; }
    public long CancelledNoshow { get; set; }
    public DateOnly? LastVisitDate { get; set; }
}
