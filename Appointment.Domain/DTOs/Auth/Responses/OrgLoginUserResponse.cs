namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Mapped from fn_get_user_for_org_login — contains the password hash and lock state
/// needed before calling fn_record_login_attempt.
/// </summary>
public sealed class OrgLoginUserResponse
{
    public int UserId { get; set; }
    public string? PasswordHash { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsEmailVerified { get; set; }
    public string? UserType { get; set; }
    public string? IdentityType { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaMethod { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public bool IsLocked { get; set; }
    public int OrgId { get; set; }
    public string? OrgCode { get; set; }
    public string? OrgName { get; set; }
    public string? OrgStatus { get; set; }
    public string? OwnershipType { get; set; }
    public string? MembershipStatus { get; set; }
}
