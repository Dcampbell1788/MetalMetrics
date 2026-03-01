using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs.Quote;

[Authorize]
public class ViewModel : PageModel
{
    private readonly IQuoteService _quoteService;
    private readonly IJobService _jobService;
    private readonly IPdfService _pdfService;
    private readonly AppDbContext _db;

    public ViewModel(IQuoteService quoteService, IJobService jobService, IPdfService pdfService, AppDbContext db)
    {
        _quoteService = quoteService;
        _jobService = jobService;
        _pdfService = pdfService;
        _db = db;
    }

    public JobEstimate Estimate { get; set; } = default!;
    public string JobSlug { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var estimate = await _quoteService.GetByJobIdAsync(job.Id);
        if (estimate == null) return NotFound();

        Estimate = estimate;
        JobSlug = slug;
        return Page();
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var estimate = await _quoteService.GetByJobIdAsync(job.Id);
        if (estimate == null) return NotFound();

        var tenantId = Guid.Parse(User.FindFirst("TenantId")!.Value);
        var tenant = await _db.Tenants.FindAsync(tenantId);
        var companyName = tenant?.CompanyName ?? "MetalMetrics";

        estimate.Job = job;
        var pdf = _pdfService.GenerateQuotePdf(companyName, job, estimate);
        return File(pdf, "application/pdf", $"Quote_{job.JobNumber}.pdf");
    }
}
