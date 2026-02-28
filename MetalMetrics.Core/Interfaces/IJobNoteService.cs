using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IJobNoteService
{
    Task<List<JobNote>> GetByJobIdAsync(Guid jobId);
    Task<JobNote> CreateAsync(JobNote note);
    Task DeleteAsync(Guid id);
}
