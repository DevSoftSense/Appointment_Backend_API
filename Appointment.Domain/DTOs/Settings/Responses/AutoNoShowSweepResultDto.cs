using System.Text.Json.Serialization;

namespace Appointment.Domain.DTOs.Settings.Responses;

public sealed class AutoNoShowSweepResultDto
{
    [JsonPropertyName("marked_count")]
    public int MarkedCount { get; set; }

    [JsonPropertyName("appointment_ids")]
    public List<long> AppointmentIds { get; set; } = [];

    [JsonPropertyName("reminders_cancelled")]
    public int RemindersCancelled { get; set; }
}
