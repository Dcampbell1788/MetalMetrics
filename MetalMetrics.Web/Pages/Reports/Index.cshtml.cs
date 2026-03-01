using System.Text.Json;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Reports;

[Authorize(Policy = "CanViewReports")]
public class IndexModel : PageModel
{
    private readonly IReportsService _reportsService;
    private readonly IPdfService _pdfService;
    private readonly AppDbContext _db;

    public IndexModel(IReportsService reportsService, IPdfService pdfService, AppDbContext db)
    {
        _reportsService = reportsService;
        _pdfService = pdfService;
        _db = db;
    }

    [BindProperty(SupportsGet = true)]
    public DateTime From { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime To { get; set; }

    public List<JobSummaryDto> Jobs { get; set; } = new();
    public List<CustomerProfitabilityDto> CustomerProfitability { get; set; } = new();
    public string CustomerProfitabilityJson { get; set; } = "[]";
    public string JobSummariesJson { get; set; } = "[]";

    // KPI summaries
    public int JobCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal AvgMargin { get; set; }
    public decimal ProfitPerJob { get; set; }

    public async Task OnGetAsync()
    {
        if (From == default)
            From = DateTime.UtcNow.AddDays(-90).Date;
        if (To == default)
            To = DateTime.UtcNow.Date;

        Jobs = await _reportsService.GetJobSummariesAsync(From, To);
        CustomerProfitability = await _reportsService.GetCustomerProfitabilityAsync(From, To);
        CustomerProfitabilityJson = JsonSerializer.Serialize(CustomerProfitability);
        JobSummariesJson = JsonSerializer.Serialize(Jobs);

        JobCount = Jobs.Count;
        TotalRevenue = Jobs.Sum(j => j.ActualRevenue);
        TotalCost = Jobs.Sum(j => j.TotalActualCost);
        TotalProfit = TotalRevenue - TotalCost;
        AvgMargin = Jobs.Count > 0 ? Jobs.Average(j => j.ActualMarginPercent) : 0;
        ProfitPerJob = Jobs.Count > 0 ? TotalProfit / Jobs.Count : 0;
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync()
    {
        if (From == default) From = DateTime.UtcNow.AddDays(-90).Date;
        if (To == default) To = DateTime.UtcNow.Date;

        var jobs = await _reportsService.GetJobSummariesAsync(From, To);
        var customers = await _reportsService.GetCustomerProfitabilityAsync(From, To);
        var jobCount = jobs.Count;
        var totalRevenue = jobs.Sum(j => j.QuotePrice);
        var avgMargin = jobs.Count > 0 ? jobs.Average(j => j.ActualMarginPercent) : 0;

        var tenantId = Guid.Parse(User.FindFirst("TenantId")!.Value);
        var tenant = await _db.Tenants.FindAsync(tenantId);
        var companyName = tenant?.CompanyName ?? "MetalMetrics";

        var pdf = _pdfService.GenerateReportsPdf(companyName, From, To, jobs, customers, jobCount, totalRevenue, avgMargin);
        return File(pdf, "application/pdf", $"Report_{From:yyyyMMdd}_{To:yyyyMMdd}.pdf");
    }
}
