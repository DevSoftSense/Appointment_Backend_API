namespace Appointment.Domain.DTOs.Auth.Responses;

/// <summary>
/// Mapped from fn_resolve_user_context (returns JSON, not a table row).
/// access_granted = false means the user is not allowed to use the Appointment product.
/// </summary>
public sealed class ResolveUserContextResponse
{
    public bool AccessGranted { get; set; }
    public string? DenialReason { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public ResolvedUserContext? User { get; set; }
    public ResolvedOrganisationContext? Organisation { get; set; }
    public ResolvedProductContext? Product { get; set; }
    public ResolvedSubscriptionContext? Subscription { get; set; }
    public ResolvedPlanContext? Plan { get; set; }
    public List<ResolvedRoleContext> Roles { get; set; } = [];
    public ResolvedLimitsContext? Limits { get; set; }
}

public sealed class ResolvedUserContext
{
    public int UserId { get; set; }
    public string? UserType { get; set; }
    public int? OwnerUserId { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public bool IsOwner { get; set; }
    public bool IsDefaultOrg { get; set; }
    public int? DefaultOrgId { get; set; }
    public bool IsRestricted { get; set; }
}

public sealed class ResolvedOrganisationContext
{
    public int OrgId { get; set; }
    public string? OrgCode { get; set; }
    public string? OrgName { get; set; }
    public string? OrgType { get; set; }
    public int? ParentOrgId { get; set; }
    public string? Status { get; set; }
    public string? CountryCode { get; set; }
    public string? Currency { get; set; }
    public string? Timezone { get; set; }
    public int? OwnerUserId { get; set; }
}

public sealed class ResolvedProductContext
{
    public int ProductId { get; set; }
    public string? ActivationType { get; set; }
    public string? Status { get; set; }
    public string? DbGroup { get; set; }
}

public sealed class ResolvedSubscriptionContext
{
    public int? SubscriptionId { get; set; }
    public bool IsEntitled { get; set; }
    public bool InGracePeriod { get; set; }
    public DateTime? ExpiresOn { get; set; }
    public DateTime? GraceUntil { get; set; }
}

public sealed class ResolvedPlanContext
{
    public int? PlanId { get; set; }
    public string? PlanCode { get; set; }
    public string? PlanName { get; set; }
    public int? SortOrder { get; set; }
    public bool IsPopular { get; set; }
}

public sealed class ResolvedRoleContext
{
    public int RoleId { get; set; }
    public string? RoleCode { get; set; }
    public string? RoleName { get; set; }
    public string? RoleLevel { get; set; }
    public bool IsSystemRole { get; set; }
    public string? Scope { get; set; }
}

public sealed class ResolvedLimitsContext
{
    public ResolvedSeatsLimit? Seats { get; set; }
    public List<ResolvedUsageCounter> Counters { get; set; } = [];
    public decimal AiCreditBalance { get; set; }
}

public sealed class ResolvedSeatsLimit
{
    public int Total { get; set; }
    public int Used { get; set; }
}

public sealed class ResolvedUsageCounter
{
    public string? CounterCode { get; set; }
    public decimal Used { get; set; }
    public decimal? Limit { get; set; }
    public string? Unit { get; set; }
    public bool Exceeded { get; set; }
}
