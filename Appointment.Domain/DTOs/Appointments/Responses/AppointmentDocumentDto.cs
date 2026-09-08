namespace Appointment.Domain.DTOs.Appointments.Responses;

public sealed class AppointmentDocumentDto
{
    public long DocumentId { get; set; }
    public long OrgId { get; set; }
    public long AppointmentId { get; set; }
    public string? DocumentName { get; set; }
    public string? FilePath { get; set; }
    public string? DocumentType { get; set; }
    public long UploadedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int? FiscalYearId { get; set; }
    public string? AppointmentNo { get; set; }
    public DateTime? AppointmentDate { get; set; }

    /// <summary>Browser URL under API static files (e.g. /Uploads/...).</summary>
    public string? FileUrl { get; set; }
}
