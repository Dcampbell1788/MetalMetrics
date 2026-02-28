using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class ProfitabilityService : IProfitabilityService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public ProfitabilityService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<JobProfitabilityReport?> CalculateAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        var job = await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == tenantId);

        if (job == null) return null;

        var estimate = job.Estimate;
        var actuals = job.Actuals;

        if (estimate == null || actuals == null) return null;

        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);
        var targetMargin = settings?.TargetMarginPercent ?? 20m;

        return Calculate(estimate, actuals, targetMargin);
    }

    public static JobProfitabilityReport Calculate(JobEstimate estimate, JobActuals actuals, decimal targetMarginPercent)
    {
        var report = new JobProfitabilityReport();

        var estLabor = estimate.EstimatedLaborHours * estimate.LaborRate;
        var actLabor = actuals.ActualLaborHours * actuals.LaborRate;
        report.LaborVariance = BuildVariance(estLabor, actLabor);

        report.MaterialVariance = BuildVariance(estimate.EstimatedMaterialCost, actuals.ActualMaterialCost);

        var estMachine = estimate.EstimatedMachineHours * estimate.MachineRate;
        var actMachine = actuals.ActualMachineHours * actuals.MachineRate;
        report.MachineVariance = BuildVariance(estMachine, actMachine);

        var estSubtotal = estLabor + estimate.EstimatedMaterialCost + estMachine;
        var actSubtotal = actLabor + actuals.ActualMaterialCost + actMachine;
        var estOverhead = estSubtotal * (estimate.OverheadPercent / 100m);
        var actOverhead = actSubtotal * (actuals.OverheadPercent / 100m);
        report.OverheadVariance = BuildVariance(estOverhead, actOverhead);

        report.TotalEstimatedCost = estSubtotal + estOverhead;
        report.TotalActualCost = actSubtotal + actOverhead;
        report.QuotedPrice = estimate.QuotePrice;
        report.ActualRevenue = actuals.ActualRevenue;

        report.EstimatedMarginDollars = estimate.QuotePrice - report.TotalEstimatedCost;
        report.EstimatedMarginPercent = estimate.QuotePrice > 0
            ? (estimate.QuotePrice - report.TotalEstimatedCost) / estimate.QuotePrice * 100m
            : 0m;

        report.ActualMarginDollars = actuals.ActualRevenue - report.TotalActualCost;
        report.ActualMarginPercent = actuals.ActualRevenue > 0
            ? (actuals.ActualRevenue - report.TotalActualCost) / actuals.ActualRevenue * 100m
            : 0m;

        report.MarginDriftDollars = report.ActualMarginDollars - report.EstimatedMarginDollars;
        report.MarginDriftPercent = report.ActualMarginPercent - report.EstimatedMarginPercent;

        if (report.ActualMarginDollars > 0)
            report.OverallVerdict = "Profit";
        else if (report.ActualMarginDollars < 0)
            report.OverallVerdict = "Loss";
        else
            report.OverallVerdict = "Break Even";

        // Warnings
        var categories = new[]
        {
            ("Labor", report.LaborVariance),
            ("Material", report.MaterialVariance),
            ("Machine", report.MachineVariance),
            ("Overhead", report.OverheadVariance)
        };

        foreach (var (name, variance) in categories)
        {
            if (variance.VariancePercent > 20m)
            {
                report.Warnings.Add($"{name} cost exceeded estimate by {variance.VariancePercent:F1}%");
            }
        }

        if (report.ActualMarginPercent < targetMarginPercent)
        {
            report.Warnings.Add($"Margin ({report.ActualMarginPercent:F1}%) is below target ({targetMarginPercent:F1}%)");
        }

        if (Math.Abs(report.MarginDriftPercent) > 10m)
        {
            report.Warnings.Add("Significant margin drift detected");
        }

        return report;
    }

    private static VarianceDetail BuildVariance(decimal estimated, decimal actual)
    {
        return new VarianceDetail
        {
            EstimatedAmount = estimated,
            ActualAmount = actual,
            VarianceDollars = actual - estimated,
            VariancePercent = estimated != 0 ? (actual - estimated) / estimated * 100m : 0m
        };
    }
}
