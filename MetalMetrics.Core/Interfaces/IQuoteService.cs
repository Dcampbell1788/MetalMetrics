using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IQuoteService
{
    Task<JobEstimate?> GetByJobIdAsync(Guid jobId);
    Task<JobEstimate> CreateAsync(JobEstimate estimate);
    void CalculateTotals(JobEstimate estimate);
}
