using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs;

[Authorize(Policy = "CanAssignJobs")]
public class AssignModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IJobAssignmentService _assignmentService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantProvider _tenantProvider;
    private readonly AppDbContext _db;

    public AssignModel(
        IJobService jobService,
        IJobAssignmentService assignmentService,
        UserManager<AppUser> userManager,
        ITenantProvider tenantProvider,
        AppDbContext db)
    {
        _jobService = jobService;
        _assignmentService = assignmentService;
        _userManager = userManager;
        _tenantProvider = tenantProvider;
        _db = db;
    }

    public Job Job { get; set; } = default!;
    public List<JobAssignment> Assignments { get; set; } = new();
    public List<AppUser> AvailableUsers { get; set; } = new();

    [BindProperty]
    public string? SelectedUserId { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        Job = job;
        Assignments = (await _assignmentService.GetByJobIdAsync(id))
            .OrderBy(a => a.User != null ? RoleOrder(a.User.Role) : 99)
            .ThenBy(a => a.User?.FullName)
            .ToList();
        await LoadAvailableUsersAsync(id);

        return Page();
    }

    public async Task<IActionResult> OnPostAssignAsync(Guid id)
    {
        if (string.IsNullOrEmpty(SelectedUserId))
        {
            TempData["Error"] = "Please select a user to assign.";
            return RedirectToPage(new { id });
        }

        // Verify job exists in this tenant
        var job = await _jobService.GetByIdAsync(id);
        if (job == null) return NotFound();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        // Verify target user belongs to the same tenant
        var assignedUser = await _userManager.FindByIdAsync(SelectedUserId);
        if (assignedUser == null || assignedUser.TenantId != currentUser.TenantId)
        {
            TempData["Error"] = "Invalid user selection.";
            return RedirectToPage(new { id });
        }

        // Verify target user role is within assignable hierarchy
        var allowedRoles = GetAssignableRoles(currentUser.Role);
        if (!allowedRoles.Contains(assignedUser.Role))
        {
            TempData["Error"] = "You do not have permission to assign this user.";
            return RedirectToPage(new { id });
        }

        await _assignmentService.AssignAsync(id, SelectedUserId, currentUser.Id);

        TempData["Success"] = $"{assignedUser.FullName} assigned to job.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(Guid id, string userId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Forbid();

        // Verify the target user is within this user's assignable role hierarchy
        var targetUser = await _userManager.FindByIdAsync(userId);
        if (targetUser == null) return NotFound();

        var allowedRoles = GetAssignableRoles(currentUser.Role);
        if (!allowedRoles.Contains(targetUser.Role))
        {
            TempData["Error"] = "You do not have permission to remove this assignment.";
            return RedirectToPage(new { id });
        }

        await _assignmentService.RemoveAsync(id, userId);
        TempData["Success"] = "Assignment removed.";
        return RedirectToPage(new { id });
    }

    private async Task LoadAvailableUsersAsync(Guid jobId)
    {
        var tenantId = _tenantProvider.TenantId;
        var assignedUserIds = Assignments.Select(a => a.UserId).ToHashSet();

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return;

        var allowedRoles = GetAssignableRoles(currentUser.Role);

        var allUsers = await _db.Users
            .Where(u => u.TenantId == tenantId)
            .ToListAsync();

        AvailableUsers = allUsers
            .Where(u => allowedRoles.Contains(u.Role) && !assignedUserIds.Contains(u.Id))
            .OrderBy(u => RoleOrder(u.Role))
            .ThenBy(u => u.FullName)
            .ToList();
    }

    private static List<AppRole> GetAssignableRoles(AppRole currentRole)
    {
        return currentRole switch
        {
            AppRole.Owner or AppRole.Admin => new List<AppRole>
            {
                AppRole.ProjectManager, AppRole.Estimator, AppRole.Foreman, AppRole.Journeyman
            },
            AppRole.ProjectManager => new List<AppRole>
            {
                AppRole.Foreman, AppRole.Journeyman
            },
            AppRole.Foreman => new List<AppRole>
            {
                AppRole.Journeyman
            },
            _ => new List<AppRole>()
        };
    }

    private static int RoleOrder(AppRole role) => role switch
    {
        AppRole.ProjectManager => 0,
        AppRole.Estimator => 1,
        AppRole.Foreman => 2,
        AppRole.Journeyman => 3,
        _ => 99
    };
}
