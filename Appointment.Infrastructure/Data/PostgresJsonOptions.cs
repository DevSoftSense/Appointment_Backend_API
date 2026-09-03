using System.Text.Json;
using System.Text.Json.Serialization;

namespace Appointment.Infrastructure.Data;

/// <summary>
/// Shared JSON deserialization options for mapping PostgreSQL function responses
/// (snake_case columns) to C# DTOs (PascalCase properties).
/// </summary>
public static class PostgresJsonOptions
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy        = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        NumberHandling              = JsonNumberHandling.AllowReadingFromString,
        DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull
    };
}
