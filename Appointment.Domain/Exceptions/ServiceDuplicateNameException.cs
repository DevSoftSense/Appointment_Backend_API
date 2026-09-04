namespace Appointment.Domain.Exceptions;

public sealed class ServiceDuplicateNameException : Exception
{
    public ServiceDuplicateNameException(string message) : base(message)
    {
    }
}
