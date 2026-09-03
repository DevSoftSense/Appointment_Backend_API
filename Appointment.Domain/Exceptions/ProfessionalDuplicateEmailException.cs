namespace Appointment.Domain.Exceptions;

public sealed class ProfessionalDuplicateEmailException : Exception
{
    public ProfessionalDuplicateEmailException(string message)
        : base(message)
    {
    }
}
