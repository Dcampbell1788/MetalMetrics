using MetalMetrics.Core.DTOs;

namespace MetalMetrics.Core.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetKpisAsync();
    Task<List<JobSummaryDto>> GetJobSummariesAsync(int limit = 20);
    Task<List<CustomerProfitabilityDto>> GetCustomerProfitabilityAsync();
    Task<List<CategoryVarianceDto>> GetCategoryVariancesAsync();
}
