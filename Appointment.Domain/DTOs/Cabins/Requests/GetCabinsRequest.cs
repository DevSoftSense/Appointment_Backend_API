namespace Appointment.Domain.DTOs.Cabins.Requests;

public sealed class GetCabinsRequest
{
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
    public int? BranchId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; }
}
