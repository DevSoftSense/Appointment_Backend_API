namespace Appointment.Domain.Exceptions;

/// <summary>
/// Thrown when a valid user is denied access to the Appointment product specifically
/// (e.g. no role assigned, product not activated, subscription expired).
/// </summary>
public sealed class ProductAccessDeniedException : Exception
{
    public string? DenialReason { get; }

    public ProductAccessDeniedException(string message, string? denialReason = null) : base(message)
    {
        DenialReason = denialReason;
    }
}
