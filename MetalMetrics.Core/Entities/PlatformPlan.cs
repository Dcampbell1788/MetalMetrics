using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;

namespace MetalMetrics.Core.Entities;

public class PlatformPlan : IAuditable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PlanInterval Interval { get; set; }
    public decimal Price { get; set; }
    public string? StripePriceId { get; set; }
    public string? StripeProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public int TrialDays { get; set; } = 14;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
