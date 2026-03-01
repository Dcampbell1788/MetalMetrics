using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Web.Pages.Jobs.Quote;

[Authorize(Policy = "CanQuote")]
public class AIModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IAIQuoteService _aiQuoteService;
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public AIModel(IJobService jobService, IAIQuoteService aiQuoteService, AppDbContext db, ITenantProvider tenantProvider)
    {
        _jobService = jobService;
        _aiQuoteService = aiQuoteService;
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
        [Display(Name = "Material Type")]
        public string MaterialType { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Material Thickness")]
        public string MaterialThickness { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Part Dimensions (L x W)")]
        public string PartDimensions { get; set; } = string.Empty;

        [Display(Name = "Sheet Size Needed")]
        public string? SheetSize { get; set; }

        [Required]
        [Display(Name = "Quantity")]
        [Range(1, 1000000)]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Operations")]
        public List<string> Operations { get; set; } = new();

        [Required]
        [Display(Name = "Complexity")]
        public string Complexity { get; set; } = "Moderate";

        [Display(Name = "Special Notes")]
        [StringLength(2000)]
        public string? SpecialNotes { get; set; }
    }

    public static readonly List<string> MaterialTypes = new()
    {
        "Mild Steel", "Stainless Steel", "Aluminum", "Galvanized", "Copper", "Other"
    };

    public static readonly List<string> OperationOptions = new()
    {
        "Laser Cut", "Brake/Bend", "Punch", "Weld", "Deburr", "Roll", "Shear", "Assembly"
    };

    public static readonly List<string> ComplexityOptions = new()
    {
        "Simple", "Moderate", "Complex"
    };

    public async Task<IActionResult> OnGetAsync(string slug)
    {
        var job = await _jobService.GetBySlugAsync(slug);
        if (job == null) return NotFound();

        JobSlug = job.Slug;
        JobNumber = job.JobNumber;
        CustomerName = job.CustomerName;
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

        var settings = await _db.TenantSettings
            .FirstOrDefaultAsync(s => s.TenantId == _tenantProvider.TenantId);

        var request = new AIQuoteRequest
        {
            MaterialType = Input.MaterialType,
            MaterialThickness = Input.MaterialThickness,
            PartDimensions = Input.PartDimensions,
            SheetSize = Input.SheetSize,
            Quantity = Input.Quantity,
            Operations = Input.Operations,
            Complexity = Input.Complexity,
            SpecialNotes = Input.SpecialNotes,
            LaborRate = settings?.DefaultLaborRate ?? 75m,
            MachineRate = settings?.DefaultMachineRate ?? 150m,
            OverheadPercent = settings?.DefaultOverheadPercent ?? 30m
        };

        var (response, error, promptSnapshot) = await _aiQuoteService.GenerateQuoteAsync(request);

        if (response == null)
        {
            TempData["Error"] = error ?? "AI quote generation failed.";
            return Page();
        }

        TempData["AIResponse"] = JsonSerializer.Serialize(response);
        TempData["AIPromptSnapshot"] = promptSnapshot;
        TempData["AIRequest"] = JsonSerializer.Serialize(request);

        return RedirectToPage("Review", new { slug });
    }
}
