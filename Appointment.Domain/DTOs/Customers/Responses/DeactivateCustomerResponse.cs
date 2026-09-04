namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class DeactivateCustomerResponse
{
    public int AccountId { get; set; }
    public string? PartyCode { get; set; }
    public string? DisplayName { get; set; }
    public string? Status { get; set; }
    public bool IsDeleted { get; set; }
}
