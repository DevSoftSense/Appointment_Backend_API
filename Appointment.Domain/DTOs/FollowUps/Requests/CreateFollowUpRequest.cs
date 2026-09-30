namespace Appointment.Domain.DTOs.FollowUps.Requests;

public sealed class CreateFollowUpRequest
{
    public DateTimeOffset StartDatetime { get; set; }
    public string? Notes { get; set; }
    public long? ProductId { get; set; }
    public long? ProfessionalId { get; set; }
    public long? CabinResourceId { get; set; }
    public int? ReminderMinutesBefore { get; set; }
}
