namespace Appointment.Domain.DTOs.Reports.Requests;

public sealed class CreateReportScheduleRequest
{
    public string ToAddress { get; set; } = "";
    /// <summary>overview | appointments | services | professionals | no_show | customers</summary>
    public string ReportTab { get; set; } = "overview";
    /// <summary>once | daily | weekly</summary>
    public string Frequency { get; set; } = "once";
    /// <summary>For daily/weekly: last N days ending today (local). Default 30.</summary>
    public int RollingDays { get; set; } = 30;
    /// <summary>For once: fixed range (ISO date).</summary>
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BranchId { get; set; }
    public int? ProfessionalId { get; set; }
    /// <summary>When to send (UTC or local with offset). Default: now.</summary>
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? Title { get; set; }
}

public sealed class GetReportSchedulesRequest
{
    public string? SendStatus { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
