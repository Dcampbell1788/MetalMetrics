using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface ITimeEntryService
{
    Task<List<JobTimeEntry>> GetByJobIdAsync(Guid jobId);
    Task<List<JobTimeEntry>> GetByUserAsync(string userId);
    Task<JobTimeEntry?> GetByIdAsync(Guid id);
    Task<JobTimeEntry> CreateAsync(JobTimeEntry entry);
    Task DeleteAsync(Guid id);
}
