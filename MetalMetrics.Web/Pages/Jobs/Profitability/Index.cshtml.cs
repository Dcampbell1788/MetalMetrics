using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Profitability;

[Authorize(Policy = "CanViewReports")]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IProfitabilityService _profitabilityService;
    private readonly IPdfService _pdfService;
    private readonly AppDbContext _db;

    public IndexModel(IJobService jobService, IProfitabilityService profitabilityService, IPdfService pdfService, AppDbContext db)
    {
        _jobService = jobService;
        _profitabilityService = profitabilityService;
        _pdfService = pdfService;
        _db = db;
    }

    public Job Job { get; set; } = default!;
    public JobProfitabilityReport? Report { get; set; }
    public bool HasActuals { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        Job = job;
        HasActuals = job.Actuals != null;

        if (HasActuals)
        {
            Report = await _profitabilityService.CalculateAsync(job.Id);
        }

        return Page();
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var report = await _profitabilityService.CalculateAsync(job.Id);
        if (report == null) return NotFound();

        var tenantId = Guid.Parse(User.FindFirst("TenantId")!.Value);
        var tenant = await _db.Tenants.FindAsync(tenantId);
        var companyName = tenant?.CompanyName ?? "MetalMetrics";

        var pdf = _pdfService.GenerateProfitabilityPdf(companyName, job, report);
        return File(pdf, "application/pdf", $"Profitability_{job.JobNumber}.pdf");
    }
}
