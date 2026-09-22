namespace Appointment.Domain.DTOs.PublicBook.Responses;

public sealed class PublicBookingLinkDto
{
    public string Token { get; set; } = "";
    /// <summary>Relative path e.g. /public/book?t=…</summary>
    public string BookingPath { get; set; } = "";
    /// <summary>Absolute UI URL using Appointment:FrontendBaseUrl</summary>
    public string BookingUrl { get; set; } = "";
}
