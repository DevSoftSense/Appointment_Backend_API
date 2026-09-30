namespace Appointment.Domain.DTOs.Payments.Requests;

public sealed class PaymentLineRequest
{
    public long? ProductId { get; set; }
    public string Description { get; set; } = "";
    public decimal Amount { get; set; }

    /// <summary>Per-line CGST % (when gst_mode = cgst_sgst).</summary>
    public decimal? CgstPercent { get; set; }

    /// <summary>Per-line SGST % (when gst_mode = cgst_sgst).</summary>
    public decimal? SgstPercent { get; set; }

    /// <summary>Per-line IGST % (when gst_mode = igst).</summary>
    public decimal? IgstPercent { get; set; }
}

/// <summary>Create or update bill payload.</summary>
public sealed class RecordPaymentRequest
{
    /// <summary>none | cgst_sgst | igst</summary>
    public string GstMode { get; set; } = "none";

    /// <summary>Legacy bill-level total %. Prefer per-line CgstPercent/SgstPercent/IgstPercent.</summary>
    public decimal GstPercent { get; set; }

    public DateOnly? BillDate { get; set; }
    public List<PaymentLineRequest> Lines { get; set; } = [];
    public decimal AmountReceived { get; set; }

    /// <summary>cash | card | upi | other</summary>
    public string PaymentMethod { get; set; } = "cash";

    public string? PaymentNote { get; set; }
    public int? FiscalYearId { get; set; }
}

public sealed class AddPaymentReceiptRequest
{
    public decimal AmountReceived { get; set; }
    public DateOnly? BillDate { get; set; }
    public string PaymentMethod { get; set; } = "cash";
    public string? PaymentNote { get; set; }
    public int? FiscalYearId { get; set; }
}
