namespace Appointment.Domain.DTOs.FollowUps.Responses;

public sealed class FollowUpListItemDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public long? ParentAppointmentId { get; set; }
    public long? RecurrenceId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long ProductId { get; set; }
    public string? ServiceName { get; set; }
    public int? ServiceDurationMinutes { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
    public long? CabinResourceId { get; set; }
    public string? CabinName { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset EndDatetime { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? Amount { get; set; }
}
