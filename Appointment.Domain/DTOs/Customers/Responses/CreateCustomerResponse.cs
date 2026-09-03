namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class CreateCustomerResponse
{
    public int AccountId { get; set; }
    public string? PartyCode { get; set; }
    public string? DisplayName { get; set; }
    public string? Status { get; set; }
}
