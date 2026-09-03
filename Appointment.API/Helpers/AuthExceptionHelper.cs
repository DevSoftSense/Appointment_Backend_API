using Npgsql;

namespace Appointment.API.Helpers;

/// <summary>
/// Translates PostgresException messages into user-facing strings.
/// </summary>
internal static class AuthExceptionHelper
{
    public static string GetUserFacingMessage(PostgresException ex)
    {
        var text = !string.IsNullOrWhiteSpace(ex.MessageText) ? ex.MessageText : ex.Message;

        if (string.IsNullOrWhiteSpace(text))
        {
            return "A database error occurred. Please try again.";
        }

        // RAISE EXCEPTION in PostgreSQL surfaces as "P0001: <message>" in some Npgsql fields.
        const string prefix = "P0001:";
        if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            text = text[prefix.Length..].Trim();
        }

        return text;
    }
}
