using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Notes;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobNoteService _noteService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public IndexModel(
        IJobService jobService,
        IJobNoteService noteService,
        IJobAssignmentService assignmentService,
        UserManager<AppUser> userManager,
        IWebHostEnvironment env)
    {
        _jobService = jobService;
        _noteService = noteService;
        _assignmentService = assignmentService;
        _userManager = userManager;
        _env = env;
    }

    public Job Job { get; set; } = default!;
    public List<JobNote> NotesList { get; set; } = new();
    public string CurrentUserId { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Note")]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }
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
        NotesList = await _noteService.GetByJobIdAsync(job.Id);
        CurrentUserId = currentUser.Id;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        if (!ModelState.IsValid)
        {
            Job = job;
            NotesList = await _noteService.GetByJobIdAsync(job.Id);
            CurrentUserId = currentUser.Id;
            return Page();
        }

        string? imageFileName = null;
        if (Input.Image != null && Input.Image.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(Input.Image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
            {
                ModelState.AddModelError("Input.Image", "Only JPG, PNG, and GIF files are allowed.");
                Job = job;
                NotesList = await _noteService.GetByJobIdAsync(job.Id);
                CurrentUserId = currentUser.Id;
                return Page();
            }

            if (Input.Image.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Input.Image", "Image must be under 5MB.");
                Job = job;
                NotesList = await _noteService.GetByJobIdAsync(job.Id);
                CurrentUserId = currentUser.Id;
                return Page();
            }

            var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "notes");
            Directory.CreateDirectory(uploadsDir);

            imageFileName = $"{Guid.NewGuid()}_{Path.GetFileName(Input.Image.FileName)}";
            var filePath = Path.Combine(uploadsDir, imageFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await Input.Image.CopyToAsync(stream);
        }

        var note = new JobNote
        {
            JobId = job.Id,
            UserId = currentUser.Id,
            TenantId = currentUser.TenantId,
            AuthorName = currentUser.FullName,
            Content = Input.Content,
            ImageFileName = imageFileName
        };

        await _noteService.CreateAsync(note);
        TempData["Success"] = "Note added.";
        return RedirectToPage(new { slug });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string slug, Guid noteId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        var notes = await _noteService.GetByJobIdAsync(job.Id);
        var note = notes.FirstOrDefault(n => n.Id == noteId);
        if (note == null) return NotFound();

        // Only the author, Admin, or Owner can delete a note
        if (note.UserId != currentUser.Id &&
            currentUser.Role != AppRole.Admin &&
            currentUser.Role != AppRole.Owner)
        {
            return Forbid();
        }

        await _noteService.DeleteAsync(noteId);
        TempData["Success"] = "Note deleted.";
        return RedirectToPage(new { slug });
    }

    private async Task<bool> CanAccessJobAsync(Guid jobId, AppUser user)
    {
        if (user.Role == AppRole.Owner || user.Role == AppRole.Admin || user.Role == AppRole.ProjectManager)
            return true;

        return await _assignmentService.IsUserAssignedAsync(jobId, user.Id);
    }
}
