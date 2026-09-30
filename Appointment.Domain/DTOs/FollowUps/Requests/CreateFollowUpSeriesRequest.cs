namespace Appointment.Domain.DTOs.FollowUps.Requests;

/// <summary>
/// Series of follow-ups: mode=count (N visits) or mode=range (until ends_on).
/// Frequency: interval_unit days|weeks + interval_value.
/// </summary>
public sealed class CreateFollowUpSeriesRequest
{
    /// <summary>count | range</summary>
    public string Mode { get; set; } = "count";

    /// <summary>days | weeks</summary>
    public string IntervalUnit { get; set; } = "weeks";

    public int IntervalValue { get; set; } = 1;

    /// <summary>Required when Mode=count (1–52).</summary>
    public int? Count { get; set; }

    /// <summary>Required when Mode=range (inclusive end date).</summary>
    public DateOnly? EndsOn { get; set; }

    public DateTimeOffset FirstStartDatetime { get; set; }

    public string? Notes { get; set; }
    public long? ProductId { get; set; }
    public long? ProfessionalId { get; set; }
    public long? CabinResourceId { get; set; }
    public int? ReminderMinutesBefore { get; set; }
}
