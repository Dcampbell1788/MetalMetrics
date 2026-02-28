using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Jobs.Quote;

[Authorize(Policy = "CanQuote")]
public class ReviewModel : PageModel
{
    private readonly IJobService _jobService;
    private readonly IQuoteService _quoteService;

    public ReviewModel(IJobService jobService, IQuoteService quoteService)
    {
        _jobService = jobService;
        _quoteService = quoteService;
    }

    public string JobNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public AIQuoteResponse AISuggestion { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty]
    public string PromptSnapshot { get; set; } = string.Empty;

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

    public IActionResult OnGet(Guid jobId)
    {
        var responseJson = TempData["AIResponse"] as string;
        var promptSnapshot = TempData["AIPromptSnapshot"] as string;
        var requestJson = TempData["AIRequest"] as string;

        if (string.IsNullOrEmpty(responseJson))
        {
            return RedirectToPage("AI", new { jobId });
        }

        var aiResponse = JsonSerializer.Deserialize<AIQuoteResponse>(responseJson);
        if (aiResponse == null)
        {
            return RedirectToPage("AI", new { jobId });
        }

        // Re-read request to get rates
        AIQuoteRequest? request = null;
        if (!string.IsNullOrEmpty(requestJson))
        {
            request = JsonSerializer.Deserialize<AIQuoteRequest>(requestJson);
        }

        AISuggestion = aiResponse;
        JobId = jobId;
        PromptSnapshot = promptSnapshot ?? string.Empty;

        // Pre-populate editable fields with AI suggestions
        Input = new InputModel
        {
            EstimatedLaborHours = aiResponse.EstimatedLaborHours,
            LaborRate = request?.LaborRate ?? 75m,
            EstimatedMaterialCost = aiResponse.EstimatedMaterialCost,
            EstimatedMachineHours = aiResponse.EstimatedMachineHours,
            MachineRate = request?.MachineRate ?? 150m,
            OverheadPercent = aiResponse.OverheadPercent,
            QuotePrice = aiResponse.SuggestedQuotePrice
        };

        // Keep AI data for display
        TempData.Keep("AIResponse");
        TempData.Keep("AIRequest");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid jobId)
    {
        var job = await _jobService.GetByIdAsync(jobId);
        if (job == null) return NotFound();

        JobId = jobId;
        JobNumber = job.JobNumber;
        CustomerName = job.CustomerName;

        // Restore AI suggestion for display
        var responseJson = TempData["AIResponse"] as string;
        if (!string.IsNullOrEmpty(responseJson))
        {
            AISuggestion = JsonSerializer.Deserialize<AIQuoteResponse>(responseJson) ?? new();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var estimate = new JobEstimate
        {
            JobId = jobId,
            EstimatedLaborHours = Input.EstimatedLaborHours,
            LaborRate = Input.LaborRate,
            EstimatedMaterialCost = Input.EstimatedMaterialCost,
            EstimatedMachineHours = Input.EstimatedMachineHours,
            MachineRate = Input.MachineRate,
            OverheadPercent = Input.OverheadPercent,
            QuotePrice = Input.QuotePrice,
            AIGenerated = true,
            AIPromptSnapshot = PromptSnapshot,
            CreatedBy = User.Identity?.Name
        };

        await _quoteService.CreateAsync(estimate);

        TempData["Success"] = $"AI-assisted quote saved for {job.JobNumber}.";
        return RedirectToPage("/Jobs/Details", new { id = jobId });
    }
}
