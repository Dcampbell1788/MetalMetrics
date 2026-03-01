using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public DashboardService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<DashboardKpiDto> GetKpisAsync()
    {
        var tenantId = _tenantProvider.TenantId;
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);
        var targetMargin = settings?.TargetMarginPercent ?? 20m;

        var allJobs = await _db.Jobs
            .Where(j => j.TenantId == tenantId)
            .CountAsync();

        var jobsThisMonth = await _db.Jobs
            .Where(j => j.TenantId == tenantId && j.CreatedAt >= monthStart)
            .CountAsync();

        var completedWithActuals = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId && j.Actuals != null && j.Estimate != null)
            .ToListAsync();

        decimal avgMargin = 0;
        int overBudget = 0;
        decimal revenueThisMonth = 0;

        foreach (var job in completedWithActuals)
        {
            var margin = job.Actuals!.ActualRevenue > 0
                ? (job.Actuals.ActualRevenue - job.Actuals.TotalActualCost) / job.Actuals.ActualRevenue * 100
                : 0;

            avgMargin += margin;

            if (margin < targetMargin)
                overBudget++;

            if (job.CompletedAt.HasValue && job.CompletedAt.Value >= monthStart)
                revenueThisMonth += job.Actuals!.ActualRevenue;
        }

        if (completedWithActuals.Count > 0)
            avgMargin /= completedWithActuals.Count;

        var totalRevenue = completedWithActuals.Sum(j => j.Actuals!.ActualRevenue);

        var inProgressCount = await _db.Jobs
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.InProgress)
            .CountAsync();

        var quotedCount = await _db.Jobs
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.Quoted)
            .CountAsync();

        var inProgressWithEstimates = await _db.Jobs
            .Include(j => j.Estimate)
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.InProgress && j.Estimate != null)
            .ToListAsync();
        var inProgressEstimatedValue = inProgressWithEstimates.Sum(j => j.Estimate!.QuotePrice);

        return new DashboardKpiDto
        {
            TotalJobs = allJobs,
            JobsThisMonth = jobsThisMonth,
            AverageMarginPercent = avgMargin,
            JobsOverBudget = overBudget,
            RevenueThisMonth = revenueThisMonth,
            TargetMarginPercent = targetMargin,
            TotalRevenue = totalRevenue,
            InProgressCount = inProgressCount,
            QuotedCount = quotedCount,
            InProgressEstimatedValue = inProgressEstimatedValue
        };
    }

    public async Task<List<JobSummaryDto>> GetJobSummariesAsync(int limit = 20)
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId && j.Actuals != null && j.Estimate != null && j.CompletedAt != null)
            .OrderByDescending(j => j.CompletedAt)
            .Take(limit)
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
                QuotePrice = j.Estimate!.QuotePrice,
                Status = j.Status.ToString(),
                ActualRevenue = j.Actuals.ActualRevenue
            };
        }).ToList();
    }

    public async Task<List<CustomerProfitabilityDto>> GetCustomerProfitabilityAsync()
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId && j.Actuals != null)
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

    public async Task<List<CategoryVarianceDto>> GetCategoryVariancesAsync()
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId && j.Actuals != null && j.Estimate != null)
            .ToListAsync();

        if (jobs.Count == 0)
            return new List<CategoryVarianceDto>();

        var laborVariances = new List<(decimal Percent, decimal Dollars)>();
        var materialVariances = new List<(decimal Percent, decimal Dollars)>();
        var machineVariances = new List<(decimal Percent, decimal Dollars)>();
        var overheadVariances = new List<(decimal Percent, decimal Dollars)>();

        foreach (var job in jobs)
        {
            var est = job.Estimate!;
            var act = job.Actuals!;

            var estLabor = est.EstimatedLaborHours * est.LaborRate;
            var actLabor = act.ActualLaborHours * act.LaborRate;
            if (estLabor > 0) laborVariances.Add(((actLabor - estLabor) / estLabor * 100, actLabor - estLabor));

            if (est.EstimatedMaterialCost > 0)
                materialVariances.Add(((act.ActualMaterialCost - est.EstimatedMaterialCost) / est.EstimatedMaterialCost * 100, act.ActualMaterialCost - est.EstimatedMaterialCost));

            var estMachine = est.EstimatedMachineHours * est.MachineRate;
            var actMachine = act.ActualMachineHours * act.MachineRate;
            if (estMachine > 0) machineVariances.Add(((actMachine - estMachine) / estMachine * 100, actMachine - estMachine));

            var estSubtotal = estLabor + est.EstimatedMaterialCost + estMachine;
            var actSubtotal = actLabor + act.ActualMaterialCost + actMachine;
            var estOverhead = estSubtotal * (est.OverheadPercent / 100m);
            var actOverhead = actSubtotal * (act.OverheadPercent / 100m);
            if (estOverhead > 0) overheadVariances.Add(((actOverhead - estOverhead) / estOverhead * 100, actOverhead - estOverhead));
        }

        var result = new List<CategoryVarianceDto>();
        AddCategory(result, "Labor", laborVariances);
        AddCategory(result, "Material", materialVariances);
        AddCategory(result, "Machine", machineVariances);
        AddCategory(result, "Overhead", overheadVariances);

        return result;
    }

    public async Task<List<AtRiskJobDto>> GetAtRiskJobsAsync(decimal thresholdPercent = 10)
    {
        var tenantId = _tenantProvider.TenantId;

        var jobs = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId && j.Status == JobStatus.InProgress && j.Estimate != null && j.Actuals != null)
            .ToListAsync();

        var atRisk = new List<AtRiskJobDto>();
        foreach (var job in jobs)
        {
            var estimatedCost = job.Estimate!.TotalEstimatedCost;
            var actualCost = job.Actuals!.TotalActualCost;
            if (estimatedCost <= 0) continue;

            var overagePercent = (actualCost - estimatedCost) / estimatedCost * 100;
            if (overagePercent > thresholdPercent)
            {
                atRisk.Add(new AtRiskJobDto
                {
                    JobId = job.Id,
                    JobNumber = job.JobNumber,
                    Slug = job.Slug,
                    CustomerName = job.CustomerName,
                    EstimatedCost = estimatedCost,
                    ActualCost = actualCost,
                    OveragePercent = overagePercent
                });
            }
        }

        return atRisk.OrderByDescending(j => j.OveragePercent).ToList();
    }

    private static void AddCategory(List<CategoryVarianceDto> list, string name, List<(decimal Percent, decimal Dollars)> variances)
    {
        if (variances.Count == 0) return;
        var avgPercent = variances.Average(v => v.Percent);
        var avgDollars = variances.Average(v => v.Dollars);
        list.Add(new CategoryVarianceDto
        {
            Category = name,
            AverageVariancePercent = avgPercent,
            Direction = avgPercent > 0 ? "Underestimate" : "Overestimate",
            JobCount = variances.Count,
            AverageDollarVariance = avgDollars
        });
    }
}
