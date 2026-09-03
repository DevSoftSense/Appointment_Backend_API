namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class AccountTypeDto
{
    public int AccountTypeId { get; set; }
    public string? PartyTypeCode { get; set; }
    public string? PartyTypeName { get; set; }
    public int? SortOrder { get; set; }
}
