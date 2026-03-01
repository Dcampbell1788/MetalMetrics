using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MetalMetrics.Core.Entities;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages;

public class LoginModel : PageModel
{
    private readonly SignInManager<AppUser> _signInManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly AppDbContext _db;

    public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, AppDbContext db)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public IActionResult OnGet(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }

        ReturnUrl = returnUrl ?? Url.Content("~/");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return Page();
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, Input.Password, Input.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
        {
            // Refresh claims for TenantId, FullName, and subscription
            var claims = await _userManager.GetClaimsAsync(user);
            if (!claims.Any(c => c.Type == "TenantId"))
            {
                await _userManager.AddClaimAsync(user, new Claim("TenantId", user.TenantId.ToString()));
            }
            if (!claims.Any(c => c.Type == "FullName"))
            {
                await _userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
            }

            // Add/refresh subscription claims from tenant
            var tenant = await _db.Tenants.FindAsync(user.TenantId);
            if (tenant != null)
            {
                var subClaim = claims.FirstOrDefault(c => c.Type == "SubscriptionStatus");
                if (subClaim != null) await _userManager.RemoveClaimAsync(user, subClaim);
                await _userManager.AddClaimAsync(user, new Claim("SubscriptionStatus", tenant.SubscriptionStatus.ToString()));

                var trialClaim = claims.FirstOrDefault(c => c.Type == "TrialEndsAt");
                if (trialClaim != null) await _userManager.RemoveClaimAsync(user, trialClaim);
                await _userManager.AddClaimAsync(user, new Claim("TrialEndsAt", tenant.TrialEndsAt.ToString("O")));

                var enabledClaim = claims.FirstOrDefault(c => c.Type == "TenantEnabled");
                if (enabledClaim != null) await _userManager.RemoveClaimAsync(user, enabledClaim);
                await _userManager.AddClaimAsync(user, new Claim("TenantEnabled", tenant.IsEnabled.ToString().ToLower()));
            }

            // Re-sign in to refresh the cookie with new claims
            await _signInManager.SignOutAsync();
            await _signInManager.SignInAsync(user, Input.RememberMe);

            return LocalRedirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }
}
