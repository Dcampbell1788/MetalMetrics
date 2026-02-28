using MetalMetrics.Core.DTOs;

namespace MetalMetrics.Core.Interfaces;

public interface IProfitabilityService
{
    Task<JobProfitabilityReport?> CalculateAsync(Guid jobId);
}
