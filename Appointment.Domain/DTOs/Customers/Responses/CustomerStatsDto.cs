using System.Text.Json.Serialization;

namespace Appointment.Domain.DTOs.Customers.Responses;

public sealed class CustomerStatsDto
{
    public long Total { get; set; }
    public long Active { get; set; }

    [JsonPropertyName("new_30d")]
    public long New30d { get; set; }

    public long Repeat { get; set; }
}
