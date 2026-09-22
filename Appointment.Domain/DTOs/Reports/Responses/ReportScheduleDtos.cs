namespace Appointment.Domain.DTOs.Reports.Responses;

public sealed class ReportScheduleItemDto
{
    public long NotificationId { get; set; }
    public int OrgId { get; set; }
    public string? Title { get; set; }
    public string? DefinitionJson { get; set; }
    public string? ToAddress { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? SendStatus { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset? CreatedDate { get; set; }
    public long TotalCount { get; set; }
}

public sealed class ReportScheduleListResponse
{
    public IReadOnlyList<ReportScheduleItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}

public sealed class ReportScheduleDueItemDto
{
    public long NotificationId { get; set; }
    public int OrgId { get; set; }
    public int AppId { get; set; }
    public string? Title { get; set; }
    public string? DefinitionJson { get; set; }
    public string? ToAddress { get; set; }
    public DateTimeOffset? ScheduledAt { get; set; }
    public string? SendStatus { get; set; }
    public long? CreatedBy { get; set; }
}

public sealed class ReportScheduleDefinitionDto
{
    public string ReportTab { get; set; } = "overview";
    public string Format { get; set; } = "csv";
    public string Frequency { get; set; } = "once";
    public int RollingDays { get; set; } = 30;
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public int? BranchId { get; set; }
    public int? ProfessionalId { get; set; }
}
