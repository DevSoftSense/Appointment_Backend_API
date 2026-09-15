namespace Appointment.Domain.DTOs.Reminders.Requests;

public sealed class GetRemindersRequest
{
    public string? Search { get; set; }
    public string? SendStatus { get; set; }
    public string? ReferenceEntity { get; set; }
    public long? AppointmentId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}

public sealed class CreateManualReminderRequest
{
    public string Title { get; set; } = "";
    public string? Message { get; set; }
    public string ToAddress { get; set; } = "";
    public DateTimeOffset ScheduledAt { get; set; }
    public long? AppointmentId { get; set; }
}
