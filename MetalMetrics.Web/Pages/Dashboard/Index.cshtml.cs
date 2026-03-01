using System.Text.Json;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Dashboard;

[Authorize(Policy = "CanViewReports")]
public class IndexModel : PageModel
{
    private readonly IDashboardService _dashboardService;

    public IndexModel(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public DashboardKpiDto Kpis { get; set; } = new();
    public List<JobSummaryDto> JobSummaries { get; set; } = new();
    public List<CustomerProfitabilityDto> CustomerProfitability { get; set; } = new();
    public List<CategoryVarianceDto> CategoryVariances { get; set; } = new();
    public List<AtRiskJobDto> AtRiskJobs { get; set; } = new();
    public string JobSummariesJson { get; set; } = "[]";
    public string CustomerProfitabilityJson { get; set; } = "[]";
    public string CategoryVariancesJson { get; set; } = "[]";

    public async Task OnGetAsync()
    {
        Kpis = await _dashboardService.GetKpisAsync();
        JobSummaries = await _dashboardService.GetJobSummariesAsync();
        CustomerProfitability = await _dashboardService.GetCustomerProfitabilityAsync();
        CategoryVariances = await _dashboardService.GetCategoryVariancesAsync();
        AtRiskJobs = await _dashboardService.GetAtRiskJobsAsync();

        JobSummariesJson = JsonSerializer.Serialize(JobSummaries);
        CustomerProfitabilityJson = JsonSerializer.Serialize(CustomerProfitability);
        CategoryVariancesJson = JsonSerializer.Serialize(CategoryVariances);
    }
}
