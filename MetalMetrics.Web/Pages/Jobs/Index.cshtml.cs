using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(
        IJobService jobService,
        IJobAssignmentService assignmentService,
        AppDbContext db,
        ITenantProvider tenantProvider,
        UserManager<AppUser> userManager)
    {
        _jobService = jobService;
        _assignmentService = assignmentService;
        _db = db;
        _tenantProvider = tenantProvider;
        _userManager = userManager;
    }

    public List<Job> Jobs { get; set; } = new();
    public decimal TargetMarginPercent { get; set; } = 20m;

    [BindProperty]
    public string? Search { get; set; }

    [BindProperty]
    public JobStatus? StatusFilter { get; set; }

    public string? SpecialFilter { get; set; }

    public List<SelectListItem> StatusOptions => Enum.GetValues<JobStatus>()
        .Select(s => new SelectListItem(s.ToString(), s.ToString()))
        .ToList();

    public async Task OnGetAsync(string? statusFilter, string? special)
    {
        // Query string overrides session
        if (!string.IsNullOrEmpty(statusFilter) && Enum.TryParse<JobStatus>(statusFilter, out var qsStatus))
        {
            StatusFilter = qsStatus;
            HttpContext.Session.SetString("JobsStatusFilter", statusFilter);
        }
        else
        {
            // Restore filters from session
            var statusStr = HttpContext.Session.GetString("JobsStatusFilter");
            StatusFilter = !string.IsNullOrEmpty(statusStr) && Enum.TryParse<JobStatus>(statusStr, out var s) ? s : null;
        }

        Search = HttpContext.Session.GetString("JobsSearch");

        if (!string.IsNullOrEmpty(special))
        {
            SpecialFilter = special;
            HttpContext.Session.SetString("JobsSpecialFilter", special);
        }
        else
        {
            SpecialFilter = HttpContext.Session.GetString("JobsSpecialFilter");
        }

        await LoadJobsAsync();
    }

    public IActionResult OnPost()
    {
        // Save filters to session, then redirect to GET (PRG pattern)
        if (!string.IsNullOrWhiteSpace(Search))
            HttpContext.Session.SetString("JobsSearch", Search);
        else
            HttpContext.Session.Remove("JobsSearch");

        if (StatusFilter.HasValue)
            HttpContext.Session.SetString("JobsStatusFilter", StatusFilter.Value.ToString());
        else
            HttpContext.Session.Remove("JobsStatusFilter");

        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        HttpContext.Session.Remove("JobsSearch");
        HttpContext.Session.Remove("JobsStatusFilter");
        HttpContext.Session.Remove("JobsSpecialFilter");
        return RedirectToPage();
    }

    private async Task LoadJobsAsync()
    {
        Jobs = await _jobService.GetAllAsync(Search, StatusFilter);

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser != null &&
            (currentUser.Role == AppRole.Journeyman || currentUser.Role == AppRole.Estimator || currentUser.Role == AppRole.Foreman))
        {
            var assignedJobIds = await _assignmentService.GetAssignedJobIdsAsync(currentUser.Id);
            var assignedSet = assignedJobIds.ToHashSet();
            Jobs = Jobs.Where(j => assignedSet.Contains(j.Id)).ToList();
        }

        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenantProvider.TenantId);
        TargetMarginPercent = settings?.TargetMarginPercent ?? 20m;

        if (SpecialFilter == "BelowTarget")
        {
            Jobs = Jobs.Where(j =>
                j.Actuals != null && j.Actuals.ActualRevenue > 0 &&
                (j.Actuals.ActualRevenue - j.Actuals.TotalActualCost) / j.Actuals.ActualRevenue * 100 < TargetMarginPercent
            ).ToList();
        }
    }
}
