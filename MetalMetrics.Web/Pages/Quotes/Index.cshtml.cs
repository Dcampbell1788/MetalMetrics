using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Quotes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(
        IJobService jobService,
        IJobAssignmentService assignmentService,
        UserManager<AppUser> userManager)
    {
        _jobService = jobService;
        _assignmentService = assignmentService;
        _userManager = userManager;
    }

    public List<Job> Quotes { get; set; } = new();

    [BindProperty]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Search = HttpContext.Session.GetString("QuotesSearch");
        await LoadQuotesAsync();
    }

    public IActionResult OnPost()
    {
        if (!string.IsNullOrWhiteSpace(Search))
            HttpContext.Session.SetString("QuotesSearch", Search);
        else
            HttpContext.Session.Remove("QuotesSearch");

        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        HttpContext.Session.Remove("QuotesSearch");
        return RedirectToPage();
    }

    private async Task LoadQuotesAsync()
    {
        Quotes = await _jobService.GetAllAsync(Search, JobStatus.Quoted);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null &&
            (currentUser.Role == AppRole.Journeyman || currentUser.Role == AppRole.Estimator || currentUser.Role == AppRole.Foreman))
        {
            var assignedJobIds = await _assignmentService.GetAssignedJobIdsAsync(currentUser.Id);
            var assignedSet = assignedJobIds.ToHashSet();
            Quotes = Quotes.Where(j => assignedSet.Contains(j.Id)).ToList();
        }
    }
}
