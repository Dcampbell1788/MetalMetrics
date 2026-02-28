using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MetalMetrics.Web.Pages.Jobs;

[Authorize(Policy = "CanManageJobs")]
public class EditModel : PageModel
{
    private readonly IJobService _jobService;

    public EditModel(IJobService jobService)
    {
        _jobService = jobService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Guid JobId { get; set; }
    public string JobNumber { get; set; } = string.Empty;

    public List<SelectListItem> StatusOptions => Enum.GetValues<JobStatus>()
        .Select(s => new SelectListItem(s.ToString(), s.ToString()))
        .ToList();

    public class InputModel
    {
        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Status")]
        public JobStatus Status { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        JobId = job.Id;
        JobNumber = job.JobNumber;
        Input = new InputModel
        {
            CustomerName = job.CustomerName,
            Description = job.Description,
            Status = job.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        if (!ModelState.IsValid)
        {
            var job2 = await _jobService.GetByIdAsync(id);
            JobId = id;
            JobNumber = job2?.JobNumber ?? string.Empty;
            return Page();
        }

        var job = await _jobService.GetByIdAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        job.CustomerName = Input.CustomerName;
        job.Description = Input.Description;

        if (Input.Status == JobStatus.Completed && job.Status != JobStatus.Completed)
        {
            job.CompletedAt = DateTime.UtcNow;
        }

        job.Status = Input.Status;
        await _jobService.UpdateAsync(job);

        TempData["Success"] = $"Job {job.JobNumber} updated successfully.";
        return RedirectToPage("Details", new { id = job.Id });
    }
}
