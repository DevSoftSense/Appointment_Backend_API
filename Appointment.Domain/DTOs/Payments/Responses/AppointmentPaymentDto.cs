namespace Appointment.Domain.DTOs.Payments.Responses;

public sealed class PaymentLineDto
{
    public long? SpTranUniqueCode { get; set; }
    public long? ProductId { get; set; }
    public string? Description { get; set; }
    public decimal? Quantity { get; set; }
    public decimal? Rate { get; set; }
    public decimal? Amount { get; set; }
    public decimal? CgstPercent { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? SgstPercent { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? IgstPercent { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? TotalAmount { get; set; }
}

public sealed class PaymentReceiptDto
{
    public long Code { get; set; }
    public decimal? Amount { get; set; }
    public DateOnly? Date { get; set; }
    public string? VchType { get; set; }
    public string? Disc { get; set; }
    public string? GCode { get; set; }
}

public sealed class PaymentBillDto
{
    public long SpUniqueCode { get; set; }
    public string? InvNo { get; set; }
    public DateOnly? InvDate { get; set; }
    public string? Description { get; set; }
    public decimal? TaxableAmount { get; set; }
    public decimal? CgstPercent { get; set; }
    public decimal? CgstAmount { get; set; }
    public decimal? SgstPercent { get; set; }
    public decimal? SgstAmount { get; set; }
    public decimal? IgstPercent { get; set; }
    public decimal? IgstAmount { get; set; }
    public decimal? GrandTotal { get; set; }
    public decimal? FinalOutstanding { get; set; }
    public decimal? AmountReceived { get; set; }
    public string? GstMode { get; set; }
    public decimal? GstPercent { get; set; }
    public List<PaymentLineDto> Lines { get; set; } = [];
    public List<PaymentReceiptDto> Receipts { get; set; } = [];
}

public sealed class AppointmentPaymentDto
{
    public long AppointmentId { get; set; }
    public string? AppointmentNo { get; set; }
    public string? AppointmentStatus { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? ServiceName { get; set; }
    public decimal? ServicePrice { get; set; }
    public string? ServiceGstMode { get; set; }
    public decimal? ServiceCgstPer { get; set; }
    public decimal? ServiceSgstPer { get; set; }
    public decimal? ServiceIgstPer { get; set; }
    public long? ProductId { get; set; }
    public string? PaymentStatus { get; set; }
    public decimal? Amount { get; set; }
    public bool CanRecord { get; set; }
    public bool CanAddPayment { get; set; }
    public bool CanVoid { get; set; }
    public PaymentBillDto? Bill { get; set; }
}
