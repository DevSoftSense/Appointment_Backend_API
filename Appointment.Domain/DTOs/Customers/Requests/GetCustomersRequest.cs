namespace Appointment.Domain.DTOs.Customers.Requests;

public sealed class GetCustomersRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public int? AccountTypeId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
