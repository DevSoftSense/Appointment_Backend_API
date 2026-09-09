namespace Appointment.Domain.DTOs.Queue.Responses;

public sealed class QueueListResponse
{
    public IReadOnlyList<QueueListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
