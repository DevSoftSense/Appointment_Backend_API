namespace Appointment.Domain.DTOs.Reminders.Responses;

public sealed class ReminderListItemDto
{
    public long NotificationId { get; set; }
    public long? OrgId { get; set; }
    public int? AppId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? ToAddress { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? SendStatus { get; set; }
    public string? NotificationType { get; set; }
    public long? AppointmentId { get; set; }
    public string? ReferenceEntity { get; set; }
    public string? ReferenceType { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? AppointmentNo { get; set; }
    public DateTimeOffset? AppointmentStart { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? ServiceName { get; set; }
    public string? ProfessionalName { get; set; }
    public int TotalCount { get; set; }
}

public sealed class ReminderListResponse
{
    public IReadOnlyList<ReminderListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
}

public sealed class ReminderStatsDto
{
    public int Total { get; set; }
    public int Scheduled { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Cancelled { get; set; }
    public int Manual { get; set; }
    public int Appointment { get; set; }
}

public sealed class ReminderDueItemDto
{
    public long NotificationId { get; set; }
    public long OrgId { get; set; }
    public int AppId { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
    public string? ToAddress { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? SendStatus { get; set; }
    public long? AppointmentId { get; set; }
    public string? ReferenceEntity { get; set; }
}
