namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class CustomerDetailDto
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
    public string? PhoneWork { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public DateOnly? AnniversaryDate { get; set; }
    public string? AcquisitionSource { get; set; }
    public DateOnly? CustomerSince { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? CompanyName { get; set; }
    public int? AccountTypeId { get; set; }
    public string? AccountTypeCode { get; set; }
    public string? AccountTypeName { get; set; }
    public string? PartyType { get; set; }
    public int? BranchId { get; set; }
    public int? FiscalYearId { get; set; }
    public string? Status { get; set; }
    public string? Remarks { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
}
