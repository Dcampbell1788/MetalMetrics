namespace MetalMetrics.Core.Entities;

public class SubscriptionEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? StripeEventId { get; set; }
    public string? Details { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
