namespace Appointment.Domain.DTOs.Payments.Responses;

public sealed class PaymentEntryListItemDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public string? AppointmentStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? ServiceName { get; set; }
    public string? ProfessionalName { get; set; }
    public long? BranchId { get; set; }
    public string? BranchName { get; set; }
    public long SpUniqueCode { get; set; }
    public string? InvNo { get; set; }
    public DateOnly? InvDate { get; set; }
    public decimal? TaxableAmount { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? AmountReceived { get; set; }
    public decimal? FinalOutstanding { get; set; }
    public bool HasBill { get; set; }
    /// <summary>bill | to_collect</summary>
    public string? EntryKind { get; set; }
}

public sealed class PaymentEntryListResponse
{
    public List<PaymentEntryListItemDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
}

public sealed class PaymentEntryStatsDto
{
    public int TotalBills { get; set; }
    public int Paid { get; set; }
    public int Partial { get; set; }
    public int Unpaid { get; set; }
    public decimal TotalBilled { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalOutstanding { get; set; }
    /// <summary>Completed visits with no payment bill yet.</summary>
    public int ToCollect { get; set; }
    public decimal ToCollectAmount { get; set; }
}
