using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public JobService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<Job>> GetAllAsync(string? search = null, JobStatus? statusFilter = null)
    {
        var tenantId = _tenantProvider.TenantId;
        var query = _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .Where(j => j.TenantId == tenantId)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(j =>
                j.CustomerName.ToLower().Contains(term) ||
                j.JobNumber.ToLower().Contains(term));
        }

        if (statusFilter.HasValue)
        {
            query = query.Where(j => j.Status == statusFilter.Value);
        }

        return await query.OrderByDescending(j => j.CreatedAt).ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        var tenantId = _tenantProvider.TenantId;
        return await _db.Jobs
            .Include(j => j.Estimate)
            .Include(j => j.Actuals)
            .FirstOrDefaultAsync(j => j.Id == id && j.TenantId == tenantId);
    }

    public async Task<Job> CreateAsync(string customerName, string? description)
    {
        var tenantId = _tenantProvider.TenantId;
        var nextNumber = await GetNextJobNumberAsync(tenantId);

        var job = new Job
        {
            CustomerName = customerName,
            Description = description,
            JobNumber = nextNumber,
            TenantId = tenantId
        };

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(Job job)
    {
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync();
    }

    private async Task<string> GetNextJobNumberAsync(Guid tenantId)
    {
        var lastJob = await _db.Jobs
            .Where(j => j.TenantId == tenantId)
            .OrderByDescending(j => j.JobNumber)
            .FirstOrDefaultAsync();

        if (lastJob == null)
        {
            return "JOB-0001";
        }

        var parts = lastJob.JobNumber.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[1], out var num))
        {
            return $"JOB-{(num + 1):D4}";
        }

        return $"JOB-0001";
    }
}
