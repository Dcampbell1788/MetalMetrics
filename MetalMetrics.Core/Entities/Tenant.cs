using MetalMetrics.Core.Enums;

namespace MetalMetrics.Core.Entities;

public class Tenant : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;

    // Subscription fields
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.Trial;
    public DateTime TrialEndsAt { get; set; }
    public DateTime? SubscriptionStartedAt { get; set; }
    public DateTime? SubscriptionEndsAt { get; set; }
    public PlanInterval? PlanInterval { get; set; }
    public bool IsEnabled { get; set; } = true;

    // Stripe fields
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public string? StripePriceId { get; set; }

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public TenantSettings? Settings { get; set; }
}
