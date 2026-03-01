using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IPlatformService
{
    Task<PlatformDashboardDto> GetPlatformDashboardAsync();
    Task<List<TenantSummaryDto>> GetAllTenantsAsync();
    Task<TenantDetailDto?> GetTenantDetailAsync(Guid tenantId);
    Task SetTenantEnabledAsync(Guid tenantId, bool enabled);
    Task<List<SubscriptionEvent>> GetSubscriptionEventsAsync(Guid? tenantId = null, int limit = 50);
}
