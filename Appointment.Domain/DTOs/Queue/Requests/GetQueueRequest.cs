namespace Appointment.Domain.DTOs.Queue.Requests;

public sealed class GetQueueRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateOnly? QueueDate { get; set; }
    public long? CustomerId { get; set; }
    public long? ProfessionalId { get; set; }
    public long? ProductId { get; set; }
    public long? BranchId { get; set; }
    public int Limit { get; set; } = 100;
    public int Offset { get; set; }
}
