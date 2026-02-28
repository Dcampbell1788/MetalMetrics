using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IJobAssignmentService
{
    Task<List<JobAssignment>> GetByJobIdAsync(Guid jobId);
    Task<List<Guid>> GetAssignedJobIdsAsync(string userId);
    Task<bool> IsUserAssignedAsync(Guid jobId, string userId);
    Task AssignAsync(Guid jobId, string userId, string assignedByUserId);
    Task RemoveAsync(Guid jobId, string userId);
}
