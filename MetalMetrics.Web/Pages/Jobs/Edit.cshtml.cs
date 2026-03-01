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

    public string JobSlug { get; set; } = string.Empty;
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

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null)
        {
            return NotFound();
        }

        JobSlug = job.Slug;
        JobNumber = job.JobNumber;
        Input = new InputModel
        {
            CustomerName = job.CustomerName,
            Description = job.Description,
            Status = job.Status
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        if (!ModelState.IsValid)
        {
            var job2 = await _jobService.GetBySlugAsync(slug);
            JobSlug = slug;
            JobNumber = job2?.JobNumber ?? string.Empty;
            return Page();
        }

        var job = await _jobService.GetBySlugAsync(slug);
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
        return RedirectToPage("Details", new { slug = job.Slug });
    }
}
