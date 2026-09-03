namespace Appointment.Domain.Exceptions;

public sealed class CustomerDuplicateEmailException : Exception
{
    public CustomerDuplicateEmailException(string message)
        : base(message)
    {
    }
}
