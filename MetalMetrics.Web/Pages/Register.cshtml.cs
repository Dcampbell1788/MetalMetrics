using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages;

public class RegisterModel : PageModel
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _db;

    public RegisterModel(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Company Name")]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Your Full Name")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToPage("/Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var tenant = new Tenant { CompanyName = Input.CompanyName };
            tenant.TenantId = tenant.Id; // Self-referencing for Tenant
            _db.Tenants.Add(tenant);

            var settings = new TenantSettings { TenantId = tenant.Id };
            _db.TenantSettings.Add(settings);

            await _db.SaveChangesAsync();

            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                FullName = Input.FullName,
                TenantId = tenant.Id,
                Role = AppRole.Admin
            };

            var result = await _userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                await transaction.RollbackAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(user, AppRole.Admin.ToString());
            await _userManager.AddClaimAsync(user, new Claim("TenantId", tenant.Id.ToString()));

            await transaction.CommitAsync();

            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToPage("/Index");
        }
        catch
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            return Page();
        }
    }
}
