namespace Appointment.Domain.DTOs.Cabins.Responses;

public sealed class CabinListResponse
{
    public IReadOnlyList<CabinListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
