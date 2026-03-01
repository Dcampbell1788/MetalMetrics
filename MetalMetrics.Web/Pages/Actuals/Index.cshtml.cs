using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Actuals;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly ITenantProvider _tenantProvider;
    private readonly AppDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(
        IJobService jobService,
        IJobAssignmentService assignmentService,
        ITenantProvider tenantProvider,
        AppDbContext db,
        UserManager<AppUser> userManager)
    {
        _jobService = jobService;
        _assignmentService = assignmentService;
        _tenantProvider = tenantProvider;
        _db = db;
        _userManager = userManager;
    }

    public List<Job> Jobs { get; set; } = new();
    public decimal TargetMarginPercent { get; set; } = 20m;

    [BindProperty]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        Search = HttpContext.Session.GetString("ActualsSearch");
        await LoadJobsAsync();
    }

    public IActionResult OnPost()
    {
        if (!string.IsNullOrWhiteSpace(Search))
            HttpContext.Session.SetString("ActualsSearch", Search);
        else
            HttpContext.Session.Remove("ActualsSearch");

        return RedirectToPage();
    }

    public IActionResult OnPostClear()
    {
        HttpContext.Session.Remove("ActualsSearch");
        return RedirectToPage();
    }

    private async Task LoadJobsAsync()
    {
        Jobs = await _jobService.GetAllAsync(Search, JobStatus.InProgress);

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
    }
}
