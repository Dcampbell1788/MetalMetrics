using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class QuoteService : IQuoteService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public QuoteService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<JobEstimate?> GetByJobIdAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobEstimates
            .Include(e => e.Job)
            .FirstOrDefaultAsync(e => e.JobId == jobId && e.TenantId == tenantId);
    }

    public async Task<JobEstimate> CreateAsync(JobEstimate estimate)
    {
        estimate.TenantId = _tenantProvider.TenantId;
        CalculateTotals(estimate);

        _db.JobEstimates.Add(estimate);
        await _db.SaveChangesAsync();
        return estimate;
    }

    public void CalculateTotals(JobEstimate estimate)
    {
        var laborCost = estimate.EstimatedLaborHours * estimate.LaborRate;
        var machineCost = estimate.EstimatedMachineHours * estimate.MachineRate;
        var subtotal = laborCost + estimate.EstimatedMaterialCost + machineCost;
        var overhead = subtotal * (estimate.OverheadPercent / 100m);

        estimate.TotalEstimatedCost = subtotal + overhead;

        estimate.EstimatedMarginPercent = estimate.QuotePrice > 0
            ? (estimate.QuotePrice - estimate.TotalEstimatedCost) / estimate.QuotePrice * 100m
            : 0m;
    }
}
