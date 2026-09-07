namespace Appointment.Domain.DTOs.Appointments.Responses;

public sealed class AppointmentDetailDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public long? OrgId { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPartyCode { get; set; }
    public string? CustomerTypeName { get; set; }
    public long ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long ProductId { get; set; }
    public string? ServiceName { get; set; }
    public int? ServiceDurationMinutes { get; set; }
    public decimal? ServicePrice { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
    public long? CabinResourceId { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset EndDatetime { get; set; }
    public string? Status { get; set; }
    public string? AppointmentType { get; set; }
    public string? Source { get; set; }
    public string? Priority { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? Amount { get; set; }
    public bool IsAllDay { get; set; }
    public string? Visibility { get; set; }
    public int? ReminderMinutesBefore { get; set; }
    public string? Notes { get; set; }
    public string? InternalNote { get; set; }
    public bool InternalNoteIsPrivate { get; set; }
    public DateOnly? FollowUpDate { get; set; }
    public string? FollowUpNotes { get; set; }
    public bool FollowUpCompleted { get; set; }
    public DateTimeOffset? CheckedInAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public long? CancelledBy { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public int? FiscalYearId { get; set; }
}
