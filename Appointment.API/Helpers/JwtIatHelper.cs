using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Appointment.API.Helpers;

/// <summary>
/// SoftOnCloud JWTs carry numeric <c>iat</c>, but JwtBearer often does not put it on
/// <see cref="ClaimsPrincipal"/>. Missing iat previously made logout revoke treat every
/// new token as revoked → user bounced back to /login immediately.
/// </summary>
public static class JwtIatHelper
{
    public static DateTimeOffset? ReadIssuedAt(HttpRequest? request, ClaimsPrincipal? user)
    {
        var unix = ReadIssuedAtUnix(request, user);
        return unix is > 1_000_000_000
            ? DateTimeOffset.FromUnixTimeSeconds(unix.Value)
            : null;
    }

    public static long? ReadIssuedAtUnix(HttpRequest? request, ClaimsPrincipal? user)
    {
        var fromClaims = TryParseUnix(
            user?.FindFirst("iat")?.Value
            ?? user?.FindFirst(JwtRegisteredClaimNames.Iat)?.Value);
        if (fromClaims is not null) return fromClaims;

        var bearer = ReadBearerToken(request);
        if (string.IsNullOrEmpty(bearer)) return null;

        // Prefer raw payload decode — always has SoftOnCloud iat even when claims omit it.
        var fromPayload = TryParseUnix(ReadPayloadField(bearer, "iat"));
        if (fromPayload is not null) return fromPayload;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(bearer);
            if (jwt.Payload.TryGetValue("iat", out var raw))
                return TryParseUnix(Convert.ToString(raw));
        }
        catch
        {
            /* ignore */
        }

        return null;
    }

    private static string? ReadBearerToken(HttpRequest? request)
    {
        if (request is null) return null;
        var auth = request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(auth)) return null;
        if (auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return auth["Bearer ".Length..].Trim();
        return auth.Trim();
    }

    private static string? ReadPayloadField(string jwt, string field)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return null;
        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(field, out var el)) return null;
            return el.ValueKind switch
            {
                JsonValueKind.Number => el.GetRawText(),
                JsonValueKind.String => el.GetString(),
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static long? TryParseUnix(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        if (long.TryParse(raw, out var unix) && unix > 1_000_000_000)
            return unix;
        return null;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var s = input.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4)
        {
            case 2: s += "=="; break;
            case 3: s += "="; break;
        }
        return Convert.FromBase64String(s);
    }
}
