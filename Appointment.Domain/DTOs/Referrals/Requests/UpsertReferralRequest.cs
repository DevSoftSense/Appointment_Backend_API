namespace Appointment.Domain.DTOs.Referrals.Requests;

public sealed class UpsertReferralRequest
{
    /// <summary>professional | customer | external | other</summary>
    public string ReferredByType { get; set; } = "";

    public long? ReferredByEmployeeId { get; set; }
    public long? ReferredByAccountId { get; set; }
    public string? ReferredByName { get; set; }
    public string? ReferredByNotes { get; set; }

    /// <summary>Optional on appointment upsert — must match appointment customer if set.</summary>
    public long? AccountId { get; set; }
}
