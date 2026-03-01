using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Attachments;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IFileUploadService _fileUploadService;

    public IndexModel(IJobService jobService, IFileUploadService fileUploadService)
    {
        _jobService = jobService;
        _fileUploadService = fileUploadService;
    }

    public Job Job { get; set; } = default!;
    public List<JobAttachment> Attachments { get; set; } = new();

    [BindProperty]
    public IFormFile? Upload { get; set; }

    [BindProperty]
    [Display(Name = "Category")]
    public string Category { get; set; } = "Document";

    [BindProperty]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        Job = job;
        Attachments = await _fileUploadService.GetByJobIdAsync(job.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        if (Upload == null || Upload.Length == 0)
        {
            TempData["Error"] = "Please select a file to upload.";
            return RedirectToPage(new { slug });
        }

        var (isValid, error) = _fileUploadService.ValidateFile(Upload);
        if (!isValid)
        {
            TempData["Error"] = error;
            return RedirectToPage(new { slug });
        }

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "";
        var userName = User.FindFirst("FullName")?.Value ?? "Unknown";

        await _fileUploadService.UploadAsync(Upload, job.Id, Category, Description, userId, userName);
        TempData["Success"] = "File uploaded successfully.";
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string slug, Guid attachmentId)
    {
        await _fileUploadService.DeleteAsync(attachmentId);
        TempData["Success"] = "Attachment deleted.";
        return RedirectToPage(new { slug });
    }
}
