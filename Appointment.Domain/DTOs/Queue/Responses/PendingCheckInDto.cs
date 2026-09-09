namespace Appointment.Domain.DTOs.Queue.Responses;

public sealed class PendingCheckInDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public long CustomerId { get; set; }
    public long ProfessionalId { get; set; }
    public long ProductId { get; set; }
    public long? BranchId { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset EndDatetime { get; set; }
    public string? Status { get; set; }
    public string? AppointmentType { get; set; }
    public string? Source { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerCode { get; set; }
    public string? ProfessionalName { get; set; }
    public string? ServiceName { get; set; }
    public int? ServiceDurationMinutes { get; set; }
}
