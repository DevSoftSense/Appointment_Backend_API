namespace Appointment.Domain.DTOs.Queue.Responses;

public sealed class QueueStatsDto
{
    public DateOnly? QueueDate { get; set; }
    public int Waiting { get; set; }
    public int Called { get; set; }
    public int InService { get; set; }
    public int Completed { get; set; }
    public int Skipped { get; set; }
    public int WalkIns { get; set; }
    public int TotalCheckedIn { get; set; }
    public int AvgWaitMinutes { get; set; }
}
