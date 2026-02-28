using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs.Actuals;

[Authorize(Policy = "CanEnterActuals")]
public class QuickModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IActualsService _actualsService;
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public QuickModel(IJobService jobService, IActualsService actualsService, AppDbContext db, ITenantProvider tenantProvider)
    {
        _jobService = jobService;
        _actualsService = actualsService;
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public Job Job { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Labor Hours")]
        [Range(0, 100000)]
        public decimal ActualLaborHours { get; set; }

        [Required]
        [Display(Name = "Material Cost ($)")]
        [Range(0, 10000000)]
        public decimal ActualMaterialCost { get; set; }

        [Required]
        [Display(Name = "Machine Hours")]
        [Range(0, 100000)]
        public decimal ActualMachineHours { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(Guid jobId)
    {
        var job = await _jobService.GetByIdAsync(jobId);
        if (job == null) return NotFound();

        Job = job;

        var existing = await _actualsService.GetByJobIdAsync(jobId);
        if (existing != null)
        {
            Input = new InputModel
            {
                ActualLaborHours = existing.ActualLaborHours,
                ActualMaterialCost = existing.ActualMaterialCost,
                ActualMachineHours = existing.ActualMachineHours,
                Notes = existing.Notes
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid jobId)
    {
        var job = await _jobService.GetByIdAsync(jobId);
        if (job == null) return NotFound();

        Job = job;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenantProvider.TenantId);

        var actuals = new JobActuals
        {
            JobId = jobId,
            ActualLaborHours = Input.ActualLaborHours,
            LaborRate = settings?.DefaultLaborRate ?? 75m,
            ActualMaterialCost = Input.ActualMaterialCost,
            ActualMachineHours = Input.ActualMachineHours,
            MachineRate = settings?.DefaultMachineRate ?? 150m,
            OverheadPercent = settings?.DefaultOverheadPercent ?? 15m,
            ActualRevenue = job.Estimate?.QuotePrice ?? 0,
            Notes = Input.Notes,
            EnteredBy = User.Identity?.Name
        };

        await _actualsService.SaveAsync(actuals);

        TempData["Success"] = $"Actuals saved for {job.JobNumber}.";
        return RedirectToPage("/Jobs/Details", new { id = jobId });
    }
}
