namespace Appointment.Domain.DTOs.Settings.Requests;

public sealed class GetLookupsRequest
{
    public string LookupKind { get; set; } = "";
    public bool IncludeInactive { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; }
}

public sealed class SaveLookupRequest
{
    public string LookupKind { get; set; } = "";
    public string LookupCode { get; set; } = "";
    public string LookupName { get; set; } = "";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateLookupRequest
{
    public string? LookupCode { get; set; }
    public string? LookupName { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class SaveAppointmentRulesRequest
{
    public bool? AutoNoShowEnabled { get; set; }
    public int? AutoNoShowGraceMinutes { get; set; }
}
