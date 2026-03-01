using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Billing;

[Authorize]
public class SubscribeModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscribeModel(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public List<PlatformPlan> Plans { get; set; } = new();

    public async Task OnGetAsync()
    {
        Plans = await _subscriptionService.GetActivePlansAsync();
    }

    public async Task<IActionResult> OnPostAsync(string priceId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return RedirectToPage("/Login");

        var successUrl = $"{Request.Scheme}://{Request.Host}/Billing/Success";
        var cancelUrl = $"{Request.Scheme}://{Request.Host}/Billing/Cancel";

        var checkoutUrl = await _subscriptionService.CreateCheckoutSessionAsync(tenantId, priceId, successUrl, cancelUrl);
        return Redirect(checkoutUrl);
    }
}
