namespace Appointment.Domain.DTOs.Payments.Requests;

public sealed class GetPaymentEntriesRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? PaymentStatus { get; set; }
    public long? BranchId { get; set; }
    public string? Search { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
