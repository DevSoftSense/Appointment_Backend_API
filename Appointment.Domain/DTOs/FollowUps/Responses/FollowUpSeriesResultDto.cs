using Appointment.Domain.DTOs.Appointments.Responses;

namespace Appointment.Domain.DTOs.FollowUps.Responses;

public sealed class FollowUpSeriesResultDto
{
    public long ParentAppointmentId { get; set; }
    public long? RecurrenceId { get; set; }
    public string? Mode { get; set; }
    public string? IntervalUnit { get; set; }
    public int IntervalValue { get; set; }
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public IReadOnlyList<AppointmentDetailDto>? Created { get; set; }
    public IReadOnlyList<FollowUpSkippedSlotDto>? Skipped { get; set; }
}

public sealed class FollowUpSkippedSlotDto
{
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset? StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public string? Reason { get; set; }
}

/// <summary>Preview uses same envelope; Created entries may be slim will_create objects.</summary>
public sealed class FollowUpSeriesPreviewDto
{
    public long ParentAppointmentId { get; set; }
    public string? Mode { get; set; }
    public string? IntervalUnit { get; set; }
    public int IntervalValue { get; set; }
    public int CreatedCount { get; set; }
    public int SkippedCount { get; set; }
    public IReadOnlyList<FollowUpPreviewSlotDto>? Created { get; set; }
    public IReadOnlyList<FollowUpSkippedSlotDto>? Skipped { get; set; }
}

public sealed class FollowUpPreviewSlotDto
{
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset? StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public string? Status { get; set; }
}
