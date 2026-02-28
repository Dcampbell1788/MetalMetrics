using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs;

[Authorize(Policy = "CanManageJobs")]
public class CreateModel : PageModel
{
    private readonly IJobService _jobService;

    public CreateModel(IJobService jobService)
    {
        _jobService = jobService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Customer Name")]
        [StringLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(2000)]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var job = await _jobService.CreateAsync(Input.CustomerName, Input.Description);
        TempData["Success"] = $"Job {job.JobNumber} created successfully.";
        return RedirectToPage("Details", new { id = job.Id });
    }
}
