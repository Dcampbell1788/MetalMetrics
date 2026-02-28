using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class JobNoteService : IJobNoteService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public JobNoteService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<JobNote>> GetByJobIdAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobNotes
            .Where(n => n.JobId == jobId && n.TenantId == tenantId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<JobNote> CreateAsync(JobNote note)
    {
        _db.JobNotes.Add(note);
        await _db.SaveChangesAsync();
        return note;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenantId = _tenantProvider.TenantId;
        var note = await _db.JobNotes
            .FirstOrDefaultAsync(n => n.Id == id && n.TenantId == tenantId);

        if (note != null)
        {
            _db.JobNotes.Remove(note);
            await _db.SaveChangesAsync();
        }
    }
}
