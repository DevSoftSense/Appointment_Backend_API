using Appointment.Domain.DTOs.Auth.Responses;

namespace Appointment.Application.Services.Interfaces;

/// <summary>
/// Abstraction for JWT access token generation.
/// Implemented in Appointment.API so that Application layer stays free of JWT package dependency.
/// </summary>
public interface IJwtTokenHelper
{
    string GenerateAccessToken(OrgLoginContextResponse context, int sessionId);
}
