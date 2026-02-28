using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public TimeEntryService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<JobTimeEntry>> GetByJobIdAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobTimeEntries
            .Include(t => t.User)
            .Where(t => t.JobId == jobId && t.TenantId == tenantId)
            .OrderByDescending(t => t.WorkDate)
            .ToListAsync();
    }

    public async Task<List<JobTimeEntry>> GetByUserAsync(string userId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobTimeEntries
            .Where(t => t.UserId == userId && t.TenantId == tenantId)
            .OrderByDescending(t => t.WorkDate)
            .ToListAsync();
    }

    public async Task<JobTimeEntry?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobTimeEntries
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);
    }

    public async Task<JobTimeEntry> CreateAsync(JobTimeEntry entry)
    {
        _db.JobTimeEntries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenantId = _tenantProvider.TenantId;
        var entry = await _db.JobTimeEntries
            .FirstOrDefaultAsync(t => t.Id == id && t.TenantId == tenantId);

        if (entry != null)
        {
            _db.JobTimeEntries.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
