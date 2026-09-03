using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Auth.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Appointment.API.Helpers;

/// <summary>
/// Generates short-lived JWT access tokens from the org login context returned by fn_get_org_login_context.
/// </summary>
public sealed class JwtTokenHelper : IJwtTokenHelper
{
    private readonly IConfiguration _configuration;

    public JwtTokenHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateAccessToken(OrgLoginContextResponse context, int sessionId)
    {
        var secret = _configuration["Jwt:Key"]
                     ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var issuer   = _configuration["Jwt:Issuer"]   ?? "Appointment.API";
        var audience = _configuration["Jwt:Audience"] ?? "Appointment.API.Users";
        var accessTokenMinutes = _configuration.GetValue<int?>("Jwt:AccessTokenMinutes") ?? 60;

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   context.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, context.Email ?? string.Empty),
            new("user_id",              context.UserId.ToString()),
            new("org_id",               context.OrgId.ToString()),
            new("session_id",           sessionId.ToString()),
            new("identity_type",        context.IdentityType ?? string.Empty),
            new("org_code",             context.OrgCode ?? string.Empty),
            new("is_shared_db_instance", context.IsSharedDbInstance.ToString().ToLowerInvariant()),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            notBefore:          DateTime.UtcNow,
            expires:            DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
