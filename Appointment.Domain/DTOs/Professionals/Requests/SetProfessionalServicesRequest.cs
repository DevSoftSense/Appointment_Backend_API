namespace Appointment.Domain.DTOs.Professionals.Requests;

public sealed class SetProfessionalServicesRequest
{
    public List<int> ProductIds { get; set; } = [];
}
