using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class UsersModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ITenantProvider _tenantProvider;

    public UsersModel(UserManager<AppUser> userManager, ITenantProvider tenantProvider)
    {
        _userManager = userManager;
        _tenantProvider = tenantProvider;
    }

    public List<UserViewModel> Users { get; set; } = new();
    public string? GeneratedPassword { get; set; }
    public string? InvitedEmail { get; set; }

    [BindProperty]
    public InviteModel Invite { get; set; } = new();

    [BindProperty]
    public RoleUpdateModel RoleUpdate { get; set; } = new();

    public List<SelectListItem> RoleOptions => Enum.GetValues<AppRole>()
        .Select(r => new SelectListItem(r.ToString(), r.ToString()))
        .ToList();

    public class InviteModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Role")]
        public AppRole Role { get; set; } = AppRole.Journeyman;
    }

    public class RoleUpdateModel
    {
        public string UserId { get; set; } = string.Empty;
        public AppRole NewRole { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsLockedOut { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostInviteAsync()
    {
        ModelState.Remove("RoleUpdate.UserId");

        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        var tenantId = _tenantProvider.TenantId;
        var tempPassword = GenerateTempPassword();

        var user = new AppUser
        {
            UserName = Invite.Email,
            Email = Invite.Email,
            FullName = Invite.FullName,
            TenantId = tenantId,
            Role = Invite.Role
        };

        var result = await _userManager.CreateAsync(user, tempPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            await LoadUsersAsync();
            return Page();
        }

        await _userManager.AddToRoleAsync(user, Invite.Role.ToString());
        await _userManager.AddClaimAsync(user,
            new System.Security.Claims.Claim("TenantId", tenantId.ToString()));
        await _userManager.AddClaimAsync(user,
            new System.Security.Claims.Claim("FullName", user.FullName));

        GeneratedPassword = tempPassword;
        InvitedEmail = Invite.Email;
        Invite = new InviteModel();

        await LoadUsersAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostChangeRoleAsync()
    {
        var user = await _userManager.FindByIdAsync(RoleUpdate.UserId);
        if (user == null || user.TenantId != _tenantProvider.TenantId)
        {
            return NotFound();
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, RoleUpdate.NewRole.ToString());
        user.Role = RoleUpdate.NewRole;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"Role updated for {user.Email}.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleLockAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null || user.TenantId != _tenantProvider.TenantId)
        {
            return NotFound();
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"{user.Email} has been reactivated.";
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            TempData["Warning"] = $"{user.Email} has been deactivated.";
        }

        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        var tenantId = _tenantProvider.TenantId;
        var users = await _userManager.Users
            .Where(u => u.TenantId == tenantId)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        Users = users.Select(u => new UserViewModel
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            Role = u.Role.ToString(),
            IsLockedOut = u.LockoutEnd > DateTimeOffset.UtcNow
        }).ToList();
    }

    private static string GenerateTempPassword()
    {
        return $"Temp{Random.Shared.Next(1000, 9999)}!";
    }
}
