using System.ComponentModel.DataAnnotations;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs.Quote;

[Authorize(Policy = "CanQuote")]
public class CreateModel : PageModel
{
    private readonly IQuoteService _quoteService;
    private readonly IJobService _jobService;
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public CreateModel(IQuoteService quoteService, IJobService jobService, AppDbContext db, ITenantProvider tenantProvider)
    {
        _quoteService = quoteService;
        _jobService = jobService;
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public string JobNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string JobSlug { get; set; } = string.Empty;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Estimated Labor Hours")]
        [Range(0, 100000)]
        public decimal EstimatedLaborHours { get; set; }

        [Required]
        [Display(Name = "Labor Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal LaborRate { get; set; }

        [Required]
        [Display(Name = "Estimated Material Cost ($)")]
        [Range(0, 10000000)]
        public decimal EstimatedMaterialCost { get; set; }

        [Required]
        [Display(Name = "Estimated Machine Hours")]
        [Range(0, 100000)]
        public decimal EstimatedMachineHours { get; set; }

        [Required]
        [Display(Name = "Machine Rate ($/hr)")]
        [Range(0, 10000)]
        public decimal MachineRate { get; set; }

        [Required]
        [Display(Name = "Overhead (%)")]
        [Range(0, 100)]
        public decimal OverheadPercent { get; set; }

        [Required]
        [Display(Name = "Quote Price ($)")]
        [Range(0, 10000000)]
        public decimal QuotePrice { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        if (job.Estimate != null)
        {
            return RedirectToPage("View", new { slug });
        }

        JobSlug = job.Slug;
        JobNumber = job.JobNumber;
        CustomerName = job.CustomerName;

        // Pre-populate from TenantSettings
        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenantProvider.TenantId);

        if (settings != null)
        {
            Input.LaborRate = settings.DefaultLaborRate;
            Input.MachineRate = settings.DefaultMachineRate;
            Input.OverheadPercent = settings.DefaultOverheadPercent;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        JobSlug = job.Slug;
        JobNumber = job.JobNumber;
        CustomerName = job.CustomerName;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var estimate = new JobEstimate
        {
            JobId = job.Id,
            EstimatedLaborHours = Input.EstimatedLaborHours,
            LaborRate = Input.LaborRate,
            EstimatedMaterialCost = Input.EstimatedMaterialCost,
            EstimatedMachineHours = Input.EstimatedMachineHours,
            MachineRate = Input.MachineRate,
            OverheadPercent = Input.OverheadPercent,
            QuotePrice = Input.QuotePrice,
            CreatedBy = User.Identity?.Name
        };

        await _quoteService.CreateAsync(estimate);

        TempData["Success"] = $"Quote created for {job.JobNumber}.";
        return RedirectToPage("/Jobs/Details", new { slug = job.Slug });
    }
}
