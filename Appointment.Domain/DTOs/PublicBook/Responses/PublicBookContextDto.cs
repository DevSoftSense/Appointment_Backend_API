namespace Appointment.Domain.DTOs.PublicBook.Responses;

public sealed class PublicBookContextDto
{
    public string Title { get; set; } = "Book an appointment";
    public string? Message { get; set; }
    public bool Ready { get; set; }

    /// <summary>Org advance booking window (1–12 months ahead).</summary>
    public int BookingWindowMonths { get; set; } = 1;

    /// <summary>YYYY-MM-DD — first bookable day (today).</summary>
    public string BookMinDate { get; set; } = "";

    /// <summary>YYYY-MM-DD — last bookable day (end of month + window).</summary>
    public string BookMaxDate { get; set; } = "";
}
