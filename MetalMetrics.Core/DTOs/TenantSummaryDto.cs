using MetalMetrics.Core.Enums;

namespace MetalMetrics.Core.DTOs;

public class TenantSummaryDto
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
}
