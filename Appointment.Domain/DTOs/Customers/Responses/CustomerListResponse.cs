namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class CustomerListResponse
{
    public IReadOnlyList<CustomerListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
