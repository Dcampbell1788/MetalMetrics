using MetalMetrics.Core.DTOs;

namespace MetalMetrics.Core.Interfaces;

public interface IReportsService
{
    Task<List<JobSummaryDto>> GetJobSummariesAsync(DateTime from, DateTime to);
    Task<List<CustomerProfitabilityDto>> GetCustomerProfitabilityAsync(DateTime from, DateTime to);
}
