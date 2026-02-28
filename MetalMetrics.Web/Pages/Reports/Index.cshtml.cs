using System.Text.Json;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Reports;

[Authorize(Policy = "CanViewReports")]
public class IndexModel : PageModel
{
    private readonly IReportsService _reportsService;

    public IndexModel(IReportsService reportsService)
    {
        _reportsService = reportsService;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime To { get; set; }

    public List<JobSummaryDto> Jobs { get; set; } = new();
    public List<CustomerProfitabilityDto> CustomerProfitability { get; set; } = new();
    public string CustomerProfitabilityJson { get; set; } = "[]";

    // KPI summaries
    public int JobCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgMargin { get; set; }

    public async Task OnGetAsync()
    {
        if (From == default)
            From = DateTime.UtcNow.AddDays(-90).Date;
        if (To == default)
            To = DateTime.UtcNow.Date;

        Jobs = await _reportsService.GetJobSummariesAsync(From, To);
        CustomerProfitability = await _reportsService.GetCustomerProfitabilityAsync(From, To);
        CustomerProfitabilityJson = JsonSerializer.Serialize(CustomerProfitability);

        JobCount = Jobs.Count;
        TotalRevenue = Jobs.Sum(j => j.QuotePrice);
        AvgMargin = Jobs.Count > 0 ? Jobs.Average(j => j.ActualMarginPercent) : 0;
    }
}
