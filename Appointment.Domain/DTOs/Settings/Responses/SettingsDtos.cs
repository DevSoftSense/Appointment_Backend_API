namespace Appointment.Domain.DTOs.Settings.Responses;

public sealed class SettingsLookupItemDto
{
    public int LookupId { get; set; }
    public int OrgId { get; set; }
    public string? LookupKind { get; set; }
    public string? LookupCode { get; set; }
    public string? LookupName { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public DateTimeOffset? UpdatedOn { get; set; }
    public long TotalCount { get; set; }
}

public sealed class SettingsMasterSummaryDto
{
    public string? LookupKind { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public long Count { get; set; }
}

public sealed class AppointmentRulesDto
{
    public bool AutoNoShowEnabled { get; set; }
    public int AutoNoShowGraceMinutes { get; set; } = 10;
}
