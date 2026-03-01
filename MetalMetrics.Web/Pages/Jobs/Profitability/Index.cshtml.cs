using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Profitability;

[Authorize(Policy = "CanViewReports")]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IProfitabilityService _profitabilityService;

    public IndexModel(IJobService jobService, IProfitabilityService profitabilityService)
    {
        _jobService = jobService;
        _profitabilityService = profitabilityService;
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
}
