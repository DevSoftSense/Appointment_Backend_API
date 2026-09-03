using System.Security.Cryptography;
using System.Text;
using Appointment.Infrastructure.Repositories.Interfaces;
using Appointment.Application.Services.Interfaces;
using Appointment.Domain.DTOs.Auth.Requests;
using Appointment.Domain.DTOs.Auth.Responses;
using Appointment.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Appointment.Application.Services.Classes;

/// <summary>
/// Implements the full login flow against SOC_SaaS_V2 auth functions.
/// Password comparison happens here in C# (BCrypt) — never in SQL.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtTokenHelper _jwtTokenHelper;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IJwtTokenHelper jwtTokenHelper)
    {
        _authRepository = authRepository;
        _configuration  = configuration;
        _logger         = logger;
        _jwtTokenHelper = jwtTokenHelper;
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        string? userAgent,
        string? deviceInfo,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (request is null) throw new ArgumentNullException(nameof(request));
            if (request.OrgId <= 0)
                throw new ArgumentException("Organisation ID is required.", nameof(request.OrgId));
            if (string.IsNullOrWhiteSpace(request.Email))
                throw new ArgumentException("Email is required.", nameof(request.Email));
            if (string.IsNullOrWhiteSpace(request.Password))
                throw new ArgumentException("Password is required.", nameof(request.Password));

            var maxFailedAttempts = _configuration.GetValue<int?>("Login:MaxFailedAttempts") ?? 5;
            var lockoutMinutes = _configuration.GetValue<int?>("Login:LockoutMinutes") ?? 15;
            var sessionDurationMinutes = request.RememberMe
                ? _configuration.GetValue<int?>("Login:RememberMeSessionDurationMinutes") ?? 43200
                : _configuration.GetValue<int?>("Login:SessionDurationMinutes") ?? 480;

            var tokenHashSecret = _configuration["Security:TokenHashSecret"]
                                  ?? _configuration["Security:OtpHashSecret"]
                                  ?? throw new InvalidOperationException(
                                      "Security:TokenHashSecret or Security:OtpHashSecret is not configured.");

            var email = request.Email.Trim();
            var user = await _authRepository.GetUserForOrgLoginAsync(request.OrgId, email, cancellationToken);

            if (user is null)
            {
                throw new LoginUnauthorizedException("Invalid organisation ID, email, or password.");
            }

            await EnsureUserTypeAsync(user, cancellationToken);
            ValidateUserLoginEligibility(user);

            var passwordValid = !string.IsNullOrWhiteSpace(user.PasswordHash)
                                && BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!passwordValid)
            {
                var failedAttempt = await _authRepository.RecordLoginAttemptAsync(
                    user.UserId,
                    "failed",
                    "invalid_password",
                    ipAddress,
                    deviceInfo,
                    userAgent,
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    maxFailedAttempts,
                    lockoutMinutes,
                    request.OrgId,
                    cancellationToken);

                if (failedAttempt.IsAccountLocked)
                {
                    throw new LoginUnauthorizedException(
                        BuildLockoutMessage(failedAttempt.LockedUntil, failedAttempt.FailedLoginAttempts));
                }

                var remaining = Math.Max(0, maxFailedAttempts - failedAttempt.FailedLoginAttempts);
                var message = remaining > 0
                    ? $"Invalid organisation ID, email, or password. {remaining} attempt(s) remaining before lockout."
                    : "Invalid organisation ID, email, or password.";

                throw new LoginUnauthorizedException(message);
            }

            var successAttempt = await _authRepository.RecordLoginAttemptAsync(
                user.UserId,
                "success",
                null,
                ipAddress,
                deviceInfo,
                userAgent,
                Guid.NewGuid(),
                Guid.NewGuid(),
                maxFailedAttempts,
                lockoutMinutes,
                request.OrgId,
                cancellationToken);

            if (!successAttempt.SessionAuthorized)
            {
                throw new LoginUnauthorizedException(
                    BuildLockoutMessage(successAttempt.LockedUntil, successAttempt.FailedLoginAttempts));
            }

            var refreshToken = GenerateRefreshToken();
            var refreshTokenHash = ComputeTokenHash(tokenHashSecret, refreshToken);

            var session = await _authRepository.CreateUserSessionAsync(
                user.UserId,
                refreshTokenHash,
                "password",
                request.OrgId,
                deviceInfo,
                userAgent,
                ipAddress,
                request.RememberMe,
                sessionDurationMinutes,
                cancellationToken);

            var context = await _authRepository.GetOrgLoginContextAsync(user.UserId, request.OrgId, cancellationToken);
            var accessToken = _jwtTokenHelper.GenerateAccessToken(context, session.SessionId);

            _logger.LogInformation(
                "User {UserId} logged in to org {OrgId}. SessionId={SessionId}",
                user.UserId, request.OrgId, session.SessionId);

            return new LoginResponse
            {
                Token        = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt    = session.ExpiresAt,
                SessionId    = session.SessionId,
                RememberMe   = request.RememberMe,
                User         = context
            };
        }
        catch (ArgumentException) { throw; }
        catch (LoginUnauthorizedException) { throw; }
        catch (PostgresException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for orgId {OrgId}", request?.OrgId);
            throw;
        }
    }

    public async Task<ResolveUserContextResponse> ResolveUserContextAsync(
        int userId,
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (userId    <= 0) throw new ArgumentException("User ID is required.", nameof(userId));
            if (orgId     <= 0) throw new ArgumentException("Organisation ID is required.", nameof(orgId));
            if (productId <= 0) throw new ArgumentException("Product ID is required.", nameof(productId));

            var context = await _authRepository.ResolveUserContextAsync(userId, orgId, productId, cancellationToken);

            if (!context.AccessGranted)
            {
                throw new ProductAccessDeniedException(
                    MapDenialReasonToMessage(context.DenialReason),
                    context.DenialReason);
            }

            return context;
        }
        catch (ArgumentException) { throw; }
        catch (ProductAccessDeniedException) { throw; }
        catch (PostgresException) { throw; }
        catch (InvalidOperationException) { throw; }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error resolving user context for userId {UserId}, orgId {OrgId}, productId {ProductId}",
                userId, orgId, productId);
            throw;
        }
    }

    private async Task EnsureUserTypeAsync(OrgLoginUserResponse user, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(user.UserType) || !string.IsNullOrWhiteSpace(user.IdentityType))
        {
            return;
        }

        user.UserType = await _authRepository.GetUserTypeByUserIdAsync(user.UserId, cancellationToken);
    }

    private static bool IsSubUser(OrgLoginUserResponse user)
    {
        var userType = user.UserType ?? user.IdentityType;
        return string.Equals(userType, "sub", StringComparison.OrdinalIgnoreCase);
    }

    private static void ValidateUserLoginEligibility(OrgLoginUserResponse user)
    {
        if (user.IsLocked)
        {
            throw new LoginUnauthorizedException(BuildLockoutMessage(user.LockedUntil, user.FailedLoginAttempts));
        }

        if (!user.IsActive)
        {
            throw new LoginUnauthorizedException("Your account is inactive. Please contact support.");
        }

        if (!user.IsEmailVerified && !IsSubUser(user))
        {
            throw new LoginUnauthorizedException("Please verify your email before signing in.");
        }

        if (user.IsRestricted)
        {
            throw new LoginUnauthorizedException("Your account is restricted. Please contact support.");
        }

        if (user.MfaEnabled)
        {
            throw new LoginUnauthorizedException(
                "Multi-factor authentication is required. Please complete MFA verification.");
        }

        if (!string.Equals(user.OrgStatus, "trial", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(user.OrgStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new LoginUnauthorizedException(
                $"Organisation is not eligible for login (status: {user.OrgStatus ?? "unknown"}).");
        }

        if (!string.Equals(user.MembershipStatus, "active", StringComparison.OrdinalIgnoreCase))
        {
            throw new LoginUnauthorizedException("You do not have active access to this organisation.");
        }
    }

    private static string GenerateRefreshToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(tokenBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string ComputeTokenHash(string secret, string token)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));

        var sb = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    private static string BuildLockoutMessage(DateTimeOffset? lockedUntil, int failedAttempts)
    {
        if (lockedUntil.HasValue && lockedUntil.Value > DateTimeOffset.UtcNow)
        {
            return $"Your account is locked until {lockedUntil.Value.ToLocalTime():g}. Please try again later.";
        }

        return failedAttempts > 0
            ? "Your account is locked due to too many failed login attempts. Please try again later."
            : "Your account is locked. Please try again later.";
    }

    private static string MapDenialReasonToMessage(string? denialReason) => denialReason switch
    {
        "user_not_found" => "User account not found.",
        "not_a_member" => "You are not a member of this organisation.",
        "membership_not_active" => "Your membership in this organisation is not active.",
        "user_deactivated" => "Your account is inactive. Please contact support.",
        "no_identity_profile" => "Your user profile is incomplete.",
        "no_role_assigned" => "You do not have a role assigned for this product.",
        "product_not_activated" => "This product is not activated for your organisation.",
        "subscription_expired" => "Your subscription for this product has expired.",
        "owner_inactive" => "Your account owner is inactive.",
        _ when denialReason?.StartsWith("organisation_", StringComparison.Ordinal) == true =>
            "This organisation is not available.",
        _ when denialReason?.StartsWith("product_", StringComparison.Ordinal) == true =>
            "This product is not available.",
        _ when denialReason?.StartsWith("user_", StringComparison.Ordinal) == true =>
            "Your account cannot access this product.",
        _ => "You do not have access to this product."
    };
}
