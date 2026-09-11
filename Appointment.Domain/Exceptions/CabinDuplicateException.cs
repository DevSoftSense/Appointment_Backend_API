namespace Appointment.Domain.Exceptions;

public sealed class CabinDuplicateException : Exception
{
    public CabinDuplicateException(string message) : base(message)
    {
    }
}
