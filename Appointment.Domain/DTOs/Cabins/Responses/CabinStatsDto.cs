namespace Appointment.Domain.DTOs.Cabins.Responses;

public sealed class CabinStatsDto
{
    public long Total { get; set; }
    public long Active { get; set; }
    public long Inactive { get; set; }
    public long BookedToday { get; set; }
}
