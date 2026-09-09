namespace Appointment.Domain.DTOs.Queue.Requests;

public sealed class QueueCheckInRequest
{
    public long AppointmentId { get; set; }
    public string? Notes { get; set; }
}
