using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface IActualsService
{
    Task<JobActuals?> GetByJobIdAsync(Guid jobId);
    Task<JobActuals> SaveAsync(JobActuals actuals);
    void CalculateTotals(JobActuals actuals);
}
