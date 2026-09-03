namespace Appointment.Domain.DTOs.Professionals.Responses;

public sealed class ProfessionalListResponse
{
    public IReadOnlyList<ProfessionalListItemDto> Items { get; set; } = [];
    public long TotalCount { get; set; }
}
