namespace Appointment.Domain.DTOs.PublicBook.Requests;

public sealed class PublicCreateCustomerRequest
{
    /// <summary>Encrypted booking token from the QR link (required). Org is resolved server-side.</summary>
    public string? Token { get; set; }

    /// <summary>Ignored when Token is present; kept for compatibility.</summary>
    public int OrgId { get; set; }

    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneMobile { get; set; }
    public string? Email { get; set; }
    public string? Gender { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Remarks { get; set; }
}

public sealed class PublicCreateAppointmentRequest
{
    /// <summary>Encrypted booking token from the QR link (required). Org is resolved server-side.</summary>
    public string? Token { get; set; }

    /// <summary>Ignored when Token is present; kept for compatibility.</summary>
    public int OrgId { get; set; }

    public long CustomerId { get; set; }
    public long ProfessionalId { get; set; }
    public long ProductId { get; set; }
    public long? BranchId { get; set; }
    public DateTimeOffset StartDatetime { get; set; }
    public DateTimeOffset? EndDatetime { get; set; }
    public DateOnly? AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public decimal? Amount { get; set; }
    public int? ReminderMinutesBefore { get; set; }
}
