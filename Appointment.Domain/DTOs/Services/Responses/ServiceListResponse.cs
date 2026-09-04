namespace Appointment.Domain.DTOs.Services.Responses;

public sealed class ServiceListResponse
{
    public IReadOnlyList<ServiceListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
