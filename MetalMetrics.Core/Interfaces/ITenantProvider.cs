namespace MetalMetrics.Core.Interfaces;

public interface ITenantProvider
{
    Guid TenantId { get; }
}
