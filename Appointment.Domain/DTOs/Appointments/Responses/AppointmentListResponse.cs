namespace Appointment.Domain.DTOs.Appointments.Responses;

public sealed class AppointmentListResponse
{
    public IReadOnlyList<AppointmentListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
