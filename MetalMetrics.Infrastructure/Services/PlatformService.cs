using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class PlatformService : IPlatformService
{
    private readonly AppDbContext _db;

    public PlatformService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PlatformDashboardDto> GetPlatformDashboardAsync()
    {
        var tenants = await _db.Tenants.Where(t => t.Id != DbSeeder.PlatformTenantId).ToListAsync();
        var plans = await _db.PlatformPlans.Where(p => p.IsActive).ToListAsync();

        var monthlyPrice = plans.FirstOrDefault(p => p.Interval == PlanInterval.Monthly)?.Price ?? 0;
        var annualPrice = plans.FirstOrDefault(p => p.Interval == PlanInterval.Annual)?.Price ?? 0;

        var activeTenants = tenants.Where(t => t.SubscriptionStatus == SubscriptionStatus.Active).ToList();
        var monthlyCount = activeTenants.Count(t => t.PlanInterval == PlanInterval.Monthly);
        var annualCount = activeTenants.Count(t => t.PlanInterval == PlanInterval.Annual);
        var mrr = (monthlyCount * monthlyPrice) + (annualCount * annualPrice / 12m);

        return new PlatformDashboardDto
        {
            TotalTenants = tenants.Count,
            ActiveTenants = activeTenants.Count,
            TrialTenants = tenants.Count(t => t.SubscriptionStatus == SubscriptionStatus.Trial),
            PastDueTenants = tenants.Count(t => t.SubscriptionStatus == SubscriptionStatus.PastDue),
            CancelledTenants = tenants.Count(t => t.SubscriptionStatus == SubscriptionStatus.Cancelled),
            MRR = mrr,
            TotalUsers = await _db.Users.CountAsync(u => u.TenantId != DbSeeder.PlatformTenantId),
            TotalJobs = await _db.Jobs.CountAsync()
        };
    }

    public async Task<List<TenantSummaryDto>> GetAllTenantsAsync()
    {
        return await _db.Tenants
            .Where(t => t.Id != DbSeeder.PlatformTenantId)
            .Select(t => new TenantSummaryDto
            {
                TenantId = t.Id,
                CompanyName = t.CompanyName,
                SubscriptionStatus = t.SubscriptionStatus,
                IsEnabled = t.IsEnabled,
                UserCount = t.Users.Count,
                JobCount = t.Jobs.Count,
                CreatedAt = t.CreatedAt,
                TrialEndsAt = t.TrialEndsAt,
                PlanInterval = t.PlanInterval
            })
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<TenantDetailDto?> GetTenantDetailAsync(Guid tenantId)
    {
        var tenant = await _db.Tenants
            .Include(t => t.Users)
            .Include(t => t.Jobs)
            .FirstOrDefaultAsync(t => t.Id == tenantId);

        if (tenant == null) return null;

        var recentEvents = await _db.SubscriptionEvents
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.OccurredAt)
            .Take(20)
            .ToListAsync();

        var totalRevenue = await _db.JobActuals
            .Where(a => a.TenantId == tenantId)
            .SumAsync(a => (decimal?)a.ActualRevenue) ?? 0;

        return new TenantDetailDto
        {
            TenantId = tenant.Id,
            CompanyName = tenant.CompanyName,
            SubscriptionStatus = tenant.SubscriptionStatus,
            IsEnabled = tenant.IsEnabled,
            UserCount = tenant.Users.Count,
            JobCount = tenant.Jobs.Count,
            CreatedAt = tenant.CreatedAt,
            TrialEndsAt = tenant.TrialEndsAt,
            PlanInterval = tenant.PlanInterval,
            SubscriptionStartedAt = tenant.SubscriptionStartedAt,
            SubscriptionEndsAt = tenant.SubscriptionEndsAt,
            StripeCustomerId = tenant.StripeCustomerId,
            StripeSubscriptionId = tenant.StripeSubscriptionId,
            StripePriceId = tenant.StripePriceId,
            TotalRevenue = totalRevenue,
            Users = tenant.Users.Select(u => new TenantUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? "",
                Role = u.Role.ToString()
            }).ToList(),
            RecentEvents = recentEvents
        };
    }

    public async Task SetTenantEnabledAsync(Guid tenantId, bool enabled)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant != null)
        {
            tenant.IsEnabled = enabled;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<SubscriptionEvent>> GetSubscriptionEventsAsync(Guid? tenantId = null, int limit = 50)
    {
        var query = _db.SubscriptionEvents.AsQueryable();
        if (tenantId.HasValue)
            query = query.Where(e => e.TenantId == tenantId.Value);

        return await query
            .OrderByDescending(e => e.OccurredAt)
            .Take(limit)
            .ToListAsync();
    }
}
