using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Quote;

[Authorize]
public class ViewModel : PageModel
{
    private readonly IQuoteService _quoteService;
    private readonly IJobService _jobService;

    public ViewModel(IQuoteService quoteService, IJobService jobService)
    {
        _quoteService = quoteService;
        _jobService = jobService;
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
}
