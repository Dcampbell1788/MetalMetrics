using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class ActualsService : IActualsService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public ActualsService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<JobActuals?> GetByJobIdAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobActuals
            .Include(a => a.Job)
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.TenantId == tenantId);
    }

    public async Task<JobActuals> SaveAsync(JobActuals actuals)
    {
        actuals.TenantId = _tenantProvider.TenantId;
        CalculateTotals(actuals);

        var existing = await _db.JobActuals
            .FirstOrDefaultAsync(a => a.JobId == actuals.JobId && a.TenantId == actuals.TenantId);

        if (existing != null)
        {
            existing.ActualLaborHours = actuals.ActualLaborHours;
            existing.LaborRate = actuals.LaborRate;
            existing.ActualMaterialCost = actuals.ActualMaterialCost;
            existing.ActualMachineHours = actuals.ActualMachineHours;
            existing.MachineRate = actuals.MachineRate;
            existing.OverheadPercent = actuals.OverheadPercent;
            existing.TotalActualCost = actuals.TotalActualCost;
            existing.ActualRevenue = actuals.ActualRevenue;
            existing.Notes = actuals.Notes;
            existing.EnteredBy = actuals.EnteredBy;
        }
        else
        {
            _db.JobActuals.Add(actuals);
        }

        await _db.SaveChangesAsync();
        return existing ?? actuals;
    }

    public void CalculateTotals(JobActuals actuals)
    {
        var laborCost = actuals.ActualLaborHours * actuals.LaborRate;
        var machineCost = actuals.ActualMachineHours * actuals.MachineRate;
        var subtotal = laborCost + actuals.ActualMaterialCost + machineCost;
        var overhead = subtotal * (actuals.OverheadPercent / 100m);

        actuals.TotalActualCost = subtotal + overhead;
    }
}
