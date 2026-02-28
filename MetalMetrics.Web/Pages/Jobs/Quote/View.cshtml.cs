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

    public ViewModel(IQuoteService quoteService)
    {
        _quoteService = quoteService;
    }

    public JobEstimate Estimate { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid jobId)
    {
        var estimate = await _quoteService.GetByJobIdAsync(jobId);
        if (estimate == null) return NotFound();

        Estimate = estimate;
        return Page();
    }
}
