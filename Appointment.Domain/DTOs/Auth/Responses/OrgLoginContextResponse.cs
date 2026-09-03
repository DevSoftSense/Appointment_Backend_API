using System.Text.Json.Serialization;

namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Mapped from fn_get_org_login_context.
/// Returned to client as part of LoginResponse.User.
/// DbConnectionString is server-side only — never sent to client or put in JWT.
/// </summary>
public sealed class OrgLoginContextResponse
{
    public int UserId { get; set; }
    public string? IdentityType { get; set; }
    public int? OwnerId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public bool IsActive { get; set; }
    public bool IsRestricted { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public bool MfaEnabled { get; set; }
    public string? MfaMethod { get; set; }
    public int OrgId { get; set; }
    public string? OrgCode { get; set; }
    public string? OrgName { get; set; }
    public string? OrgStatus { get; set; }
    public string? OrgType { get; set; }
    public bool IsOrgOwner { get; set; }
    // Legacy shape kept by fn_get_org_login_context for API backward compatibility.
    public string? OwnershipType { get; set; }
    public string? MembershipStatus { get; set; }

    /// <summary>Server-side only — never exposed in API responses or JWT.</summary>
    [JsonIgnore]
    public string? DbConnectionString { get; set; }

    public bool IsSharedDbInstance { get; set; }
}
