namespace Appointment.Domain.DTOs.Queue.Responses;

public sealed class QueueListItemDto
{
    public long QueueEntryId { get; set; }
    public long? OrgId { get; set; }
    public string? QueueNo { get; set; }
    public int? QueuePosition { get; set; }
    public long? ProductId { get; set; }
    public DateOnly? QueueDate { get; set; }
    public DateTimeOffset CheckInTime { get; set; }
    public DateTimeOffset? CalledTime { get; set; }
    public DateTimeOffset? ServiceStartTime { get; set; }
    public DateTimeOffset? ServiceEndTime { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public long? BranchId { get; set; }
    public long CustomerId { get; set; }
    public long? ProfessionalId { get; set; }
    public long? AppointmentId { get; set; }
    public int? FiscalYearId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerCode { get; set; }
    public string? AccountTypeCode { get; set; }
    public string? AccountTypeName { get; set; }
    public string? ProfessionalName { get; set; }
    public string? ServiceName { get; set; }
    public int? ServiceDurationMinutes { get; set; }
    public string? BranchName { get; set; }
    public string? AppointmentNo { get; set; }
    public DateTimeOffset? AppointmentStart { get; set; }
    public DateTimeOffset? AppointmentEnd { get; set; }
    public string? AppointmentStatus { get; set; }
    public string? AppointmentType { get; set; }
    public int? WaitMinutes { get; set; }
    public long TotalCount { get; set; }

    // Walk-in extras (add_walk_in only)
    public string? SlotMode { get; set; }
    public bool? IsAfterHours { get; set; }
    public DateTimeOffset? SuggestedStart { get; set; }
    public DateTimeOffset? SuggestedEnd { get; set; }
}
