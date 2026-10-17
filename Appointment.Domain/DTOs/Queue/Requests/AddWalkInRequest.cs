namespace Appointment.Domain.DTOs.Queue.Requests;

public sealed class AddWalkInRequest
{
    public long CustomerId { get; set; }
    public long? ProfessionalId { get; set; }
    public long ProductId { get; set; }
    public long? BranchId { get; set; }
    /// <summary>Maps to appointment.cabin_resource_id on walk-in create (existing column).</summary>
    public long? CabinResourceId { get; set; }
    public DateOnly? QueueDate { get; set; }
    public int? ExpectedDurationMinutes { get; set; }
    public string? Notes { get; set; }
}
