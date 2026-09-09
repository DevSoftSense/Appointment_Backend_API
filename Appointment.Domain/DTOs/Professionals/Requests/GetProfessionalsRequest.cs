namespace Appointment.Domain.DTOs.Professionals.Requests;

public sealed class GetProfessionalsRequest
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    /// <summary>When set, only professionals who offer this service (tab_employee_service).</summary>
    public int? ProductId { get; set; }
    public int Limit { get; set; } = 50;
    public int Offset { get; set; } = 0;
}
