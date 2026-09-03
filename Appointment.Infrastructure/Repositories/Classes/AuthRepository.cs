using System.Text.Json;
using Appointment.Infrastructure.Repositories.Interfaces;
using Appointment.Domain.DTOs.Auth.Responses;
using Appointment.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace Appointment.Infrastructure.Repositories.Classes;

/// <summary>
/// Calls PostgreSQL auth functions in SOC_SaaS_V2.
/// Primary access is via fn_*; GetUserTypeByUserIdAsync uses a minimal platform-table lookup.
/// </summary>
public sealed class AuthRepository : IAuthRepository
{
    private readonly MasterDatabaseHelper _db;
    private readonly ILogger<AuthRepository> _logger;

    public AuthRepository(MasterDatabaseHelper db, ILogger<AuthRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ─── Step 1: fn_get_user_for_org_login ───────────────────────────────────

    public async Task<OrgLoginUserResponse?> GetUserForOrgLoginAsync(
        int orgId,
        string email,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "public.fn_get_user_for_org_login",
                new NpgsqlParameter { Value = orgId,  NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = email,  NpgsqlDbType = NpgsqlDbType.Varchar }
            );

            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            return JsonSerializer.Deserialize<OrgLoginUserResponse>(json, PostgresJsonOptions.Options);
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_get_user_for_org_login for orgId {OrgId}", orgId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_get_user_for_org_login response for orgId {OrgId}", orgId);
            throw new InvalidOperationException("Login lookup response could not be parsed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserForOrgLoginAsync for orgId {OrgId}", orgId);
            throw;
        }
    }

    // ─── Step 2: fn_record_login_attempt ─────────────────────────────────────

    public async Task<LoginAttemptResponse> RecordLoginAttemptAsync(
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
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "public.fn_record_login_attempt",
                new NpgsqlParameter { Value = userId,                                   NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = loginStatus,                              NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = "password",                               NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)failureReason   ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)ipAddress       ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)deviceInfo      ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)userAgent       ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)requestId       ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Uuid },
                new NpgsqlParameter { Value = (object?)correlationId   ?? DBNull.Value, NpgsqlDbType = NpgsqlDbType.Uuid },
                new NpgsqlParameter { Value = maxFailedAttempts,                        NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = lockoutMinutes,                           NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = orgId,                                    NpgsqlDbType = NpgsqlDbType.Integer }
            );

            return JsonSerializer.Deserialize<LoginAttemptResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_record_login_attempt returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_record_login_attempt for userId {UserId}", userId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_record_login_attempt response for userId {UserId}", userId);
            throw new InvalidOperationException("Login attempt response could not be parsed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in RecordLoginAttemptAsync for userId {UserId}", userId);
            throw;
        }
    }

    // ─── Step 3: fn_create_user_session ──────────────────────────────────────

    public async Task<UserSessionResponse> CreateUserSessionAsync(
        int userId,
        string refreshTokenHash,
        string loginMethod,
        int orgId,
        string? deviceInfo,
        string? userAgent,
        string? ipAddress,
        bool isTrustedDevice,
        int sessionDurationMinutes,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "public.fn_create_user_session",
                new NpgsqlParameter { Value = userId,                                  NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = refreshTokenHash,                        NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = loginMethod,                             NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = orgId,                                   NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = (object?)deviceInfo  ?? DBNull.Value,    NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)userAgent   ?? DBNull.Value,    NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = (object?)ipAddress   ?? DBNull.Value,    NpgsqlDbType = NpgsqlDbType.Varchar },
                new NpgsqlParameter { Value = isTrustedDevice,                         NpgsqlDbType = NpgsqlDbType.Boolean },
                new NpgsqlParameter { Value = sessionDurationMinutes,                  NpgsqlDbType = NpgsqlDbType.Integer }
            );

            return JsonSerializer.Deserialize<UserSessionResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_create_user_session returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_create_user_session for userId {UserId}", userId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_create_user_session response for userId {UserId}", userId);
            throw new InvalidOperationException("Session creation response could not be parsed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateUserSessionAsync for userId {UserId}", userId);
            throw;
        }
    }

    // ─── Step 4: fn_get_org_login_context ────────────────────────────────────

    public async Task<OrgLoginContextResponse> GetOrgLoginContextAsync(
        int userId,
        int orgId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            var json = await _db.ExecuteSingleRowTableFunctionAsJsonAsync(
                "public.fn_get_org_login_context",
                new NpgsqlParameter { Value = userId, NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = orgId,  NpgsqlDbType = NpgsqlDbType.Integer }
            );

            return JsonSerializer.Deserialize<OrgLoginContextResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_get_org_login_context returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error in fn_get_org_login_context for userId {UserId}", userId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize fn_get_org_login_context response for userId {UserId}", userId);
            throw new InvalidOperationException("Login context response could not be parsed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetOrgLoginContextAsync for userId {UserId}", userId);
            throw;
        }
    }

    // ─── Authorization: fn_resolve_user_context ───────────────────────────────

    public async Task<ResolveUserContextResponse> ResolveUserContextAsync(
        int userId,
        int orgId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            // fn_resolve_user_context returns JSON (not a table row)
            var json = await _db.ExecuteJsonFunctionAsync(
                "public.fn_resolve_user_context",
                new NpgsqlParameter { Value = userId,     NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = orgId,      NpgsqlDbType = NpgsqlDbType.Integer },
                new NpgsqlParameter { Value = productId,  NpgsqlDbType = NpgsqlDbType.Integer }
            );

            return JsonSerializer.Deserialize<ResolveUserContextResponse>(json, PostgresJsonOptions.Options)
                   ?? throw new InvalidOperationException("fn_resolve_user_context returned no data");
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex,
                "PostgreSQL error in fn_resolve_user_context for userId {UserId}, orgId {OrgId}, productId {ProductId}",
                userId, orgId, productId);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex,
                "Failed to deserialize fn_resolve_user_context response for userId {UserId}, productId {ProductId}",
                userId, productId);
            throw new InvalidOperationException("User context response could not be parsed.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error in ResolveUserContextAsync for userId {UserId}, productId {ProductId}",
                userId, productId);
            throw;
        }
    }

    public async Task<string?> GetUserTypeByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;

        try
        {
            const string sql = """
                SELECT user_type
                FROM public.tab_users
                WHERE user_id = $1
                  AND is_deleted = false
                LIMIT 1
                """;

            return await _db.ExecuteScalarSqlAsync(
                sql,
                new NpgsqlParameter { Value = userId, NpgsqlDbType = NpgsqlDbType.Integer });
        }
        catch (PostgresException ex)
        {
            _logger.LogError(ex, "PostgreSQL error fetching user type for userId {UserId}", userId);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserTypeByUserIdAsync for userId {UserId}", userId);
            throw;
        }
    }
}
