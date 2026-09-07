namespace Appointment.Domain.DTOs.Appointments.Requests;

public sealed class CancelAppointmentRequest
{
    public string? CancellationReason { get; set; }
}
