using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class ReportsService : IReportsService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public ReportsService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<JobSummaryDto>> GetJobSummariesAsync(DateTime from, DateTime to)
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId
                && j.CompletedAt != null
                && j.CompletedAt >= from
                && j.CompletedAt <= to
                && j.Actuals != null
                && j.Estimate != null)
            .OrderByDescending(j => j.CompletedAt)
            .ToListAsync();

        return jobs.Select(j =>
        {
            var margin = j.Actuals!.ActualRevenue > 0
                ? (j.Actuals.ActualRevenue - j.Actuals.TotalActualCost) / j.Actuals.ActualRevenue * 100
                : 0;
            return new JobSummaryDto
            {
                JobId = j.Id,
                JobNumber = j.JobNumber,
                Slug = j.Slug,
                CustomerName = j.CustomerName,
                TotalEstimatedCost = j.Estimate!.TotalEstimatedCost,
                TotalActualCost = j.Actuals.TotalActualCost,
                ActualMarginPercent = margin,
                CompletedAt = j.CompletedAt!.Value,
                IsProfitable = j.Actuals.ActualRevenue > j.Actuals.TotalActualCost,
                QuotePrice = j.Estimate.QuotePrice,
                Status = j.Status.ToString(),
                ActualRevenue = j.Actuals.ActualRevenue
            };
        }).ToList();
    }

    public async Task<List<CustomerProfitabilityDto>> GetCustomerProfitabilityAsync(DateTime from, DateTime to)
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId
                && j.CompletedAt != null
                && j.CompletedAt >= from
                && j.CompletedAt <= to
                && j.Actuals != null)
            .ToListAsync();

        return jobs
            .GroupBy(j => j.CustomerName)
            .Select(g =>
            {
                var totalRevenue = g.Sum(j => j.Actuals!.ActualRevenue);
                var totalCost = g.Sum(j => j.Actuals!.TotalActualCost);
                var profitLoss = totalRevenue - totalCost;
                var marginPct = totalRevenue > 0 ? profitLoss / totalRevenue * 100 : 0;

                return new CustomerProfitabilityDto
                {
                    CustomerName = g.Key,
                    JobCount = g.Count(),
                    TotalRevenue = totalRevenue,
                    TotalCost = totalCost,
                    ProfitLoss = profitLoss,
                    MarginPercent = marginPct
                };
            })
            .OrderByDescending(c => c.ProfitLoss)
            .ToList();
    }
}
