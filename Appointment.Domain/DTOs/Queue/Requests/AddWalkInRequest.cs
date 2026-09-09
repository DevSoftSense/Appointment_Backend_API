namespace Appointment.Domain.DTOs.Queue.Requests;

public sealed class AddWalkInRequest
{
    public long CustomerId { get; set; }
    public long ProfessionalId { get; set; }
    public long ProductId { get; set; }
    public long? BranchId { get; set; }
    public DateOnly? QueueDate { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public string? Notes { get; set; }
}
