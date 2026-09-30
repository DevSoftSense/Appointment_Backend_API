namespace Appointment.Domain.DTOs.Referrals.Responses;

public sealed class ReferralDto
{
    public long ReferralId { get; set; }
    public long? OrgId { get; set; }
    public long AccountId { get; set; }
    public long? AppointmentId { get; set; }
    public string? ReferredByType { get; set; }
    public long? ReferredByEmployeeId { get; set; }
    public long? ReferredByAccountId { get; set; }
    public string? ReferredByName { get; set; }
    public string? ReferredByNotes { get; set; }
    public string? ReferredByDisplayName { get; set; }
    public string? ReferredByEmployeeCode { get; set; }
    public string? ReferredByPartyCode { get; set; }
    public long? CreatedBy { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
