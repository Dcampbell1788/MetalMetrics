using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;

namespace MetalMetrics.Core.Interfaces;

public interface IJobService
{
    Task<List<Job>> GetAllAsync(string? search = null, JobStatus? statusFilter = null);
    Task<Job?> GetByIdAsync(Guid id);
    Task<Job?> GetByJobNumberAsync(string jobNumber);
    Task<Job?> GetBySlugAsync(string slug);
    Task<Job> CreateAsync(string customerName, string? description);
    Task UpdateAsync(Job job);
}
