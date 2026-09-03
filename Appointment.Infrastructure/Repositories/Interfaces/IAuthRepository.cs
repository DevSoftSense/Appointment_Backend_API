using Appointment.Domain.DTOs.Auth.Responses;

namespace Appointment.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Calls PostgreSQL functions in SOC_SaaS_V2 for the login flow.
/// All methods map 1-to-1 to a specific fn_* function.
/// </summary>
public interface IAuthRepository
{
    Task<OrgLoginUserResponse?> GetUserForOrgLoginAsync(
        int orgId,
        string email,
        CancellationToken cancellationToken = default);

    Task<LoginAttemptResponse> RecordLoginAttemptAsync(
        int userId,
        string loginStatus,
        string? failureReason,
        string? ipAddress,
        string? deviceInfo,
        string? userAgent,
        Guid? requestId,
        Guid? correlationId,
        int maxFailedAttempts,
        int lockoutMinutes,
        int orgId,
        CancellationToken cancellationToken = default);

    Task<UserSessionResponse> CreateUserSessionAsync(
        int userId,
        string refreshTokenHash,
        string loginMethod,
        int orgId,
        string? deviceInfo,
        string? userAgent,
        string? ipAddress,
        bool isTrustedDevice,
        int sessionDurationMinutes,
        CancellationToken cancellationToken = default);

    Task<OrgLoginContextResponse> GetOrgLoginContextAsync(
        int userId,
        int orgId,
        CancellationToken cancellationToken = default);

    Task<ResolveUserContextResponse> ResolveUserContextAsync(
        int userId,
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fallback lookup when fn_get_user_for_org_login does not return user_type.
    /// </summary>
    Task<string?> GetUserTypeByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default);
}
