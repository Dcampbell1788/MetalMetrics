using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Admin;

[Authorize(Policy = "AdminOnly")]
public class SettingsModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public SettingsModel(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Default Labor Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal DefaultLaborRate { get; set; }

        [Required]
        [Display(Name = "Default Machine Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal DefaultMachineRate { get; set; }

        [Required]
        [Display(Name = "Default Overhead (%)")]
        [Range(0, 100)]
        public decimal DefaultOverheadPercent { get; set; }

        [Required]
        [Display(Name = "Target Margin (%)")]
        [Range(0, 100)]
        public decimal TargetMarginPercent { get; set; }
    }

    public async Task OnGetAsync()
    {
        var settings = await GetSettingsAsync();
        Input = new InputModel
        {
            DefaultLaborRate = settings.DefaultLaborRate,
            DefaultMachineRate = settings.DefaultMachineRate,
            DefaultOverheadPercent = settings.DefaultOverheadPercent,
            TargetMarginPercent = settings.TargetMarginPercent
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var settings = await GetSettingsAsync();
        settings.DefaultLaborRate = Input.DefaultLaborRate;
        settings.DefaultMachineRate = Input.DefaultMachineRate;
        settings.DefaultOverheadPercent = Input.DefaultOverheadPercent;
        settings.TargetMarginPercent = Input.TargetMarginPercent;

        await _db.SaveChangesAsync();

        TempData["Success"] = "Settings saved successfully.";
        return RedirectToPage();
    }

    private async Task<TenantSettings> GetSettingsAsync()
    {
        var tenantId = _tenantProvider.TenantId;
        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        if (settings == null)
        {
            settings = new TenantSettings { TenantId = tenantId };
            _db.TenantSettings.Add(settings);
            await _db.SaveChangesAsync();
        }

        return settings;
    }
}
