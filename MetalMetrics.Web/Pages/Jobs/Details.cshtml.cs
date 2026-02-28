using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;

    public DetailsModel(
        IJobService jobService,
        IJobAssignmentService assignmentService,
        UserManager<AppUser> userManager)
    {
        _jobService = jobService;
        _assignmentService = assignmentService;
        _userManager = userManager;
    }

    public Job Job { get; set; } = default!;
    public bool IsAssigned { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        if (!await CanAccessJobAsync(id, currentUser))
            return Forbid();

        Job = job;
        IsAssigned = await _assignmentService.IsUserAssignedAsync(id, currentUser.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(Guid id)
    {
        if (!IsManagerRole())
            return Forbid();

        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        if (job.Status != JobStatus.Quoted)
        {
            TempData["Error"] = "Job can only be started from Quoted status.";
            return RedirectToPage(new { id });
        }

        job.Status = JobStatus.InProgress;
        await _jobService.UpdateAsync(job);

        TempData["Success"] = $"Job {job.JobNumber} is now In Progress.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostCompleteAsync(Guid id)
    {
        if (!IsManagerRole())
            return Forbid();

        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        if (job.Status != JobStatus.InProgress)
        {
            TempData["Error"] = "Job can only be completed from In Progress status.";
            return RedirectToPage(new { id });
        }

        if (job.Actuals == null)
        {
            TempData["Warning"] = "Please enter actuals before completing this job.";
            return RedirectToPage("/Jobs/Actuals/Enter", new { jobId = id });
        }

        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        await _jobService.UpdateAsync(job);

        TempData["Success"] = $"Job {job.JobNumber} marked as Completed.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostInvoiceAsync(Guid id)
    {
        if (!IsManagerRole())
            return Forbid();

        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        if (job.Status != JobStatus.Completed)
        {
            TempData["Error"] = "Job can only be invoiced from Completed status.";
            return RedirectToPage(new { id });
        }

        if (job.Actuals == null)
        {
            TempData["Warning"] = "Actuals required before marking as invoiced.";
            return RedirectToPage("/Jobs/Actuals/Enter", new { jobId = id });
        }

        job.Status = JobStatus.Invoiced;
        await _jobService.UpdateAsync(job);

        TempData["Success"] = $"Job {job.JobNumber} marked as Invoiced.";
        return RedirectToPage(new { id });
    }

    private bool IsManagerRole()
    {
        return User.IsInRole("Admin") || User.IsInRole("Owner") || User.IsInRole("ProjectManager");
    }

    private async Task<bool> CanAccessJobAsync(Guid jobId, AppUser user)
    {
        if (user.Role == AppRole.Owner || user.Role == AppRole.Admin || user.Role == AppRole.ProjectManager)
            return true;

        return await _assignmentService.IsUserAssignedAsync(jobId, user.Id);
    }
}
