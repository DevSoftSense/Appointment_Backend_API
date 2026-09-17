namespace Appointment.Domain.DTOs.PublicBook.Responses;

public sealed class PublicBookingLinkDto
{
    public string Token { get; set; } = "";
    public string BookingPath { get; set; } = "";
}
