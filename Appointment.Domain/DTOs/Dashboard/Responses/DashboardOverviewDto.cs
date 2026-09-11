namespace Appointment.Domain.DTOs.Dashboard.Responses;

public sealed class DashboardOverviewDto
{
    public DateOnly? AsOfDate { get; set; }
    public string? Period { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public DashboardKpisDto Kpis { get; set; } = new();
    public IReadOnlyList<DashboardNamedCountDto> StatusBreakdown { get; set; } = [];
    public IReadOnlyList<DashboardTypeCountDto> TypeBreakdown { get; set; } = [];
    public IReadOnlyList<DashboardTrendDayDto> Trend { get; set; } = [];
    public IReadOnlyList<DashboardTopProfessionalDto> TopProfessionals { get; set; } = [];
    public IReadOnlyList<DashboardScheduleItemDto> TodaySchedule { get; set; } = [];
    public IReadOnlyList<DashboardScheduleItemDto> Upcoming { get; set; } = [];
    public DashboardGlanceDto Glance { get; set; } = new();
}

public sealed class DashboardKpisDto
{
    public long TodayTotal { get; set; }
    public long TodayCompleted { get; set; }
    public long TodayUpcoming { get; set; }
    public long TodayWalkIns { get; set; }
    public long TodayCancelledNoshow { get; set; }
    public decimal TodayRevenue { get; set; }
    public long YesterdayTotal { get; set; }
    public long YesterdayCompleted { get; set; }
    public long YesterdayCancelledNoshow { get; set; }
    public decimal YesterdayRevenue { get; set; }
}

public sealed class DashboardNamedCountDto
{
    public string? Status { get; set; }
    public long Count { get; set; }
}

public sealed class DashboardTypeCountDto
{
    public string? AppointmentType { get; set; }
    public long Count { get; set; }
}

public sealed class DashboardTrendDayDto
{
    public DateOnly Day { get; set; }
    public long Completed { get; set; }
    public long Cancelled { get; set; }
    public long NoShow { get; set; }
    public long Upcoming { get; set; }
    public long Total { get; set; }
}

public sealed class DashboardTopProfessionalDto
{
    public long ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long Count { get; set; }
}

public sealed class DashboardScheduleItemDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long? ProfessionalId { get; set; }
    public string? ProfessionalName { get; set; }
    public long? ProductId { get; set; }
    public string? ServiceName { get; set; }
    public long? CabinResourceId { get; set; }
    public string? CabinName { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public string? Status { get; set; }
    public string? AppointmentType { get; set; }
    public string? Source { get; set; }
    public decimal? Amount { get; set; }
}

public sealed class DashboardGlanceDto
{
    public decimal? NoShowRate { get; set; }
    public decimal? CompletionRate { get; set; }
    public decimal? UtilizationRate { get; set; }
    public decimal? AvgWaitMinutes { get; set; }
    public decimal? RepeatCustomersPct { get; set; }
    public decimal? Satisfaction { get; set; }
}
