using Appointment.Domain.DTOs.Auth.Requests;
using Appointment.Domain.DTOs.Auth.Responses;

namespace Appointment.Application.Services.Interfaces;

/// <summary>
/// Business logic for authentication and product-level authorization.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Executes the full 4-step login flow:
    /// fn_get_user_for_org_login → bcrypt verify → fn_record_login_attempt
    /// → fn_create_user_session → fn_get_org_login_context → JWT.
    /// </summary>
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        string? deviceInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calls fn_resolve_user_context to verify the user is entitled to use
    /// the Appointment product. Call this on every product entry point.
    /// </summary>
    Task<ResolveUserContextResponse> ResolveUserContextAsync(
        int userId,
        int orgId,
        int productId,
        CancellationToken cancellationToken = default);
}
