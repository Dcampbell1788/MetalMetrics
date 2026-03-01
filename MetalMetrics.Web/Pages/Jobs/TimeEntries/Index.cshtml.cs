using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.TimeEntries;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly ITimeEntryService _timeEntryService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;

    public IndexModel(
        IJobService jobService,
        ITimeEntryService timeEntryService,
        IJobAssignmentService assignmentService,
        UserManager<AppUser> userManager)
    {
        _jobService = jobService;
        _timeEntryService = timeEntryService;
        _assignmentService = assignmentService;
        _userManager = userManager;
    }

    public Job Job { get; set; } = default!;
    public List<JobTimeEntry> Entries { get; set; } = new();
    public bool CanLogTime { get; set; }
    public string CurrentUserId { get; set; } = string.Empty;
    public Dictionary<string, string> UserNames { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Work Date")]
        [DataType(DataType.Date)]
        public DateTime WorkDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Hours Worked")]
        [Range(0.25, 24)]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        if (!await CanAccessJobAsync(job.Id, currentUser))
            return Forbid();

        Job = job;
        await LoadEntriesAsync(job.Id, currentUser);

        // Foreman and Journeyman can log time on active jobs only
        CanLogTime = (currentUser.Role == AppRole.Foreman || currentUser.Role == AppRole.Journeyman)
            && job.Status != JobStatus.Quoted && job.Status != JobStatus.Invoiced;
        CurrentUserId = currentUser.Id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        if (currentUser.Role != AppRole.Foreman && currentUser.Role != AppRole.Journeyman)
        {
            TempData["Error"] = "Only Foremen and Journeymen can log time.";
            return RedirectToPage(new { slug });
        }

        if (job.Status == JobStatus.Quoted || job.Status == JobStatus.Invoiced)
        {
            TempData["Error"] = "Time cannot be logged on jobs that are Quoted or Invoiced.";
            return RedirectToPage(new { slug });
        }

        if (!ModelState.IsValid)
        {
            Job = job;
            await LoadEntriesAsync(job.Id, currentUser);
            CanLogTime = true;
            CurrentUserId = currentUser.Id;
            return Page();
        }

        var entry = new JobTimeEntry
        {
            JobId = job.Id,
            UserId = currentUser.Id,
            TenantId = currentUser.TenantId,
            HoursWorked = Input.HoursWorked,
            WorkDate = Input.WorkDate,
            Notes = Input.Notes
        };

        await _timeEntryService.CreateAsync(entry);
        TempData["Success"] = "Time entry logged.";
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string slug, Guid entryId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        var entry = await _timeEntryService.GetByIdAsync(entryId);
        if (entry != null && entry.UserId == currentUser.Id)
        {
            await _timeEntryService.DeleteAsync(entryId);
            TempData["Success"] = "Time entry deleted.";
        }

        return RedirectToPage(new { slug });
    }

    private async Task LoadEntriesAsync(Guid jobId, AppUser currentUser)
    {
        var allEntries = await _timeEntryService.GetByJobIdAsync(jobId);

        // Journeyman/Foreman see only their own entries; managers see all
        if (currentUser.Role == AppRole.Journeyman || currentUser.Role == AppRole.Foreman)
        {
            Entries = allEntries.Where(e => e.UserId == currentUser.Id).ToList();
        }
        else
        {
            Entries = allEntries;
        }

        // Build user name lookup from included nav property
        foreach (var entry in Entries)
        {
            if (entry.User != null && !UserNames.ContainsKey(entry.UserId))
            {
                UserNames[entry.UserId] = entry.User.FullName;
            }
        }
    }

    private async Task<bool> CanAccessJobAsync(Guid jobId, AppUser user)
    {
        if (user.Role == AppRole.Owner || user.Role == AppRole.Admin || user.Role == AppRole.ProjectManager)
            return true;

        return await _assignmentService.IsUserAssignedAsync(jobId, user.Id);
    }
}
