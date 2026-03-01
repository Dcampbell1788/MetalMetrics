using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Actuals;

[Authorize(Policy = "CanEnterActuals")]
public class EnterModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IActualsService _actualsService;

    public EnterModel(IJobService jobService, IActualsService actualsService)
    {
        _jobService = jobService;
        _actualsService = actualsService;
    }

    public Job Job { get; set; } = default!;
    public JobEstimate? Estimate { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Actual Labor Hours")]
        [Range(0, 100000)]
        public decimal ActualLaborHours { get; set; }

        [Required]
        [Display(Name = "Labor Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal LaborRate { get; set; }

        [Required]
        [Display(Name = "Actual Material Cost ($)")]
        [Range(0, 10000000)]
        public decimal ActualMaterialCost { get; set; }

        [Required]
        [Display(Name = "Actual Machine Hours")]
        [Range(0, 100000)]
        public decimal ActualMachineHours { get; set; }

        [Required]
        [Display(Name = "Machine Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal MachineRate { get; set; }

        [Required]
        [Display(Name = "Overhead (%)")]
        [Range(0, 100)]
        public decimal OverheadPercent { get; set; }

        [Required]
        [Display(Name = "Actual Revenue ($)")]
        [Range(0, 10000000)]
        public decimal ActualRevenue { get; set; }

        [Display(Name = "Notes")]
        [StringLength(2000)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        Job = job;
        Estimate = job.Estimate;

        var existing = await _actualsService.GetByJobIdAsync(job.Id);
        if (existing != null)
        {
            Input = new InputModel
            {
                ActualLaborHours = existing.ActualLaborHours,
                LaborRate = existing.LaborRate,
                ActualMaterialCost = existing.ActualMaterialCost,
                ActualMachineHours = existing.ActualMachineHours,
                MachineRate = existing.MachineRate,
                OverheadPercent = existing.OverheadPercent,
                ActualRevenue = existing.ActualRevenue,
                Notes = existing.Notes
            };
        }
        else if (Estimate != null)
        {
            Input = new InputModel
            {
                LaborRate = Estimate.LaborRate,
                MachineRate = Estimate.MachineRate,
                OverheadPercent = Estimate.OverheadPercent,
                ActualRevenue = Estimate.QuotePrice
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        Job = job;
        Estimate = job.Estimate;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var actuals = new JobActuals
        {
            JobId = job.Id,
            ActualLaborHours = Input.ActualLaborHours,
            LaborRate = Input.LaborRate,
            ActualMaterialCost = Input.ActualMaterialCost,
            ActualMachineHours = Input.ActualMachineHours,
            MachineRate = Input.MachineRate,
            OverheadPercent = Input.OverheadPercent,
            ActualRevenue = Input.ActualRevenue,
            Notes = Input.Notes,
            EnteredBy = User.Identity?.Name
        };

        await _actualsService.SaveAsync(actuals);

        TempData["Success"] = $"Actuals saved for {job.JobNumber}.";
        return RedirectToPage("/Jobs/Details", new { slug = job.Slug });
    }
}
