namespace Appointment.Domain.DTOs.Appointments.Requests;

public sealed class GetAppointmentsRequest
{
    public string? Search { get; set; }
    /// <summary>Exact status or group: upcoming, completed, cancelled, no_show, cancelled_noshow, walk_ins.</summary>
    public string? Status { get; set; }
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public long? CustomerId { get; set; }
    public long? ProfessionalId { get; set; }
    public long? ProductId { get; set; }
    public long? BranchId { get; set; }
    public string? Source { get; set; }
    public string? AppointmentType { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
