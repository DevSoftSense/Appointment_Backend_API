namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class CustomerListItemDto
{
    public int AccountId { get; set; }
    public string? PartyCode { get; set; }
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Salutation { get; set; }
    public string? Email { get; set; }
    public string? PhoneMobile { get; set; }
    public string? PhoneMobileAlt { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public int? AccountTypeId { get; set; }
    public string? PartyType { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public long TotalCount { get; set; }
}
