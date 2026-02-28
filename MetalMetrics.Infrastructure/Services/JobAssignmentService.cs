using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class JobAssignmentService : IJobAssignmentService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public JobAssignmentService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<JobAssignment>> GetByJobIdAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobAssignments
            .Include(a => a.User)
            .Where(a => a.JobId == jobId && a.TenantId == tenantId)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetAssignedJobIdsAsync(string userId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobAssignments
            .Where(a => a.UserId == userId && a.TenantId == tenantId)
            .Select(a => a.JobId)
            .ToListAsync();
    }

    public async Task<bool> IsUserAssignedAsync(Guid jobId, string userId)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.JobAssignments
            .AnyAsync(a => a.JobId == jobId && a.UserId == userId && a.TenantId == tenantId);
    }

    public async Task AssignAsync(Guid jobId, string userId, string assignedByUserId)
    {
        var tenantId = _tenantProvider.TenantId;
        var exists = await _db.JobAssignments
            .AnyAsync(a => a.JobId == jobId && a.UserId == userId && a.TenantId == tenantId);

        if (exists) return;

        var assignment = new JobAssignment
        {
            JobId = jobId,
            UserId = userId,
            AssignedByUserId = assignedByUserId,
            TenantId = tenantId
        };

        _db.JobAssignments.Add(assignment);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid jobId, string userId)
    {
        var tenantId = _tenantProvider.TenantId;
        var assignment = await _db.JobAssignments
            .FirstOrDefaultAsync(a => a.JobId == jobId && a.UserId == userId && a.TenantId == tenantId);

        if (assignment != null)
        {
            _db.JobAssignments.Remove(assignment);
            await _db.SaveChangesAsync();
        }
    }
}
