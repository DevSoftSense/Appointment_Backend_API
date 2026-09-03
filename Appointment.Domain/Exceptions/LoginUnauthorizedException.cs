namespace Appointment.Domain.Exceptions;

/// <summary>
/// Thrown when login is denied — wrong credentials, locked account, inactive org, etc.
/// </summary>
public sealed class LoginUnauthorizedException : Exception
{
    public LoginUnauthorizedException(string message) : base(message)
    {
    }
}
