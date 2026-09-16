namespace Appointment.Domain.DTOs.Reports.Responses;

public sealed class ReportOverviewDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public DateOnly? PrevFromDate { get; set; }
    public DateOnly? PrevToDate { get; set; }
    public ReportKpisDto Kpis { get; set; } = new();
    public ReportKpisDto PrevKpis { get; set; } = new();
    public IReadOnlyList<ReportTrendDayDto> Trend { get; set; } = [];
    public IReadOnlyList<ReportNamedCountDto> ByStatus { get; set; } = [];
    public IReadOnlyList<ReportNamedCountDto> BySource { get; set; } = [];
    public IReadOnlyList<ReportDayOfWeekDto> ByDayOfWeek { get; set; } = [];
    public IReadOnlyList<ReportRankItemDto> TopServices { get; set; } = [];
    public IReadOnlyList<ReportRankItemDto> TopProfessionals { get; set; } = [];
    public ReportInsightsDto Insights { get; set; } = new();
}

public sealed class ReportKpisDto
{
    public long Total { get; set; }
    public long Completed { get; set; }
    public long Upcoming { get; set; }
    public long NoShow { get; set; }
    public long Cancelled { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class ReportInsightsDto
{
    public decimal CompletionRate { get; set; }
    public decimal NoShowRate { get; set; }
}

public sealed class ReportTrendDayDto
{
    public string? Day { get; set; }
    public long Total { get; set; }
    public long Completed { get; set; }
    public long NoShow { get; set; }
    public long Cancelled { get; set; }
}

public sealed class ReportNamedCountDto
{
    public string? Status { get; set; }
    public string? Source { get; set; }
    public long Count { get; set; }
}

public sealed class ReportDayOfWeekDto
{
    public int DayOfWeek { get; set; }
    public string? DayName { get; set; }
    public long Count { get; set; }
}

public sealed class ReportRankItemDto
{
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int? ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long Count { get; set; }
    public decimal Percent { get; set; }
}

public sealed class ReportAppointmentsDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public long Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public IReadOnlyList<ReportAppointmentRowDto> Items { get; set; } = [];
}

public sealed class ReportAppointmentRowDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public string? AppointmentDate { get; set; }
    public DateTimeOffset? StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? Status { get; set; }
    public string? Source { get; set; }
    public string? AppointmentType { get; set; }
    public decimal? Amount { get; set; }
    public int? BranchId { get; set; }
}

public sealed class ReportServicesDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public IReadOnlyList<ReportServiceRowDto> Items { get; set; } = [];
}

public sealed class ReportServiceRowDto
{
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public int DurationMinutes { get; set; }
    public long Total { get; set; }
    public long Completed { get; set; }
    public long NoShow { get; set; }
    public decimal Percent { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class ReportProfessionalsDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public IReadOnlyList<ReportProfessionalRowDto> Items { get; set; } = [];
}

public sealed class ReportProfessionalRowDto
{
    public int? ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long Total { get; set; }
    public long Completed { get; set; }
    public long NoShow { get; set; }
    public decimal NoShowRate { get; set; }
    public decimal Revenue { get; set; }
}

public sealed class ReportNoShowDto
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public ReportNoShowSummaryDto Summary { get; set; } = new();
    public IReadOnlyList<ReportNoShowByProDto> ByProfessional { get; set; } = [];
    public IReadOnlyList<ReportNoShowByServiceDto> ByService { get; set; } = [];
    public IReadOnlyList<ReportNoShowByDayDto> ByDay { get; set; } = [];
}

public sealed class ReportNoShowSummaryDto
{
    public long Total { get; set; }
    public long NoShow { get; set; }
    public long Completed { get; set; }
    public long Cancelled { get; set; }
    public decimal NoShowRate { get; set; }
}

public sealed class ReportNoShowByProDto
{
    public int? ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long NoShow { get; set; }
    public long Total { get; set; }
    public decimal NoShowRate { get; set; }
}

public sealed class ReportNoShowByServiceDto
{
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public long NoShow { get; set; }
    public long Total { get; set; }
}

public sealed class ReportNoShowByDayDto
{
    public string? Day { get; set; }
    public long NoShow { get; set; }
    public long Total { get; set; }
}
