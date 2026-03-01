using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;

namespace MetalMetrics.Core.DTOs;

public class TenantDetailDto
{
    public Guid TenantId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public SubscriptionStatus SubscriptionStatus { get; set; }
    public bool IsEnabled { get; set; }
    public int UserCount { get; set; }
    public int JobCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime TrialEndsAt { get; set; }
    public PlanInterval? PlanInterval { get; set; }
    public DateTime? SubscriptionStartedAt { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<TenantUserDto> Users { get; set; } = new();
    public List<SubscriptionEvent> RecentEvents { get; set; } = new();
}

public class TenantUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
