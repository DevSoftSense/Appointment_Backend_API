namespace Appointment.Domain.DTOs.Appointments.Requests;

public sealed class CreateAppointmentRequest
{
    public long CustomerId { get; set; }
    public long ProfessionalId { get; set; }
    public long ProductId { get; set; }
    public long? BranchId { get; set; }
    public long? CabinResourceId { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? Status { get; set; }
    public string? AppointmentType { get; set; }
    public string? Source { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? Amount { get; set; }
    public bool? IsAllDay { get; set; }
    public string? Visibility { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public string? InternalNote { get; set; }
    public int? FiscalYearId { get; set; }
}
