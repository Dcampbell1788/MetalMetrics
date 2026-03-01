using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Billing;

[Authorize(Policy = "AdminOnly")]
public class PortalModel : PageModel
{
    private readonly ISubscriptionService _subscriptionService;

    public PortalModel(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        if (!Guid.TryParse(tenantIdClaim, out var tenantId))
            return RedirectToPage("/Login");

        try
        {
            var returnUrl = $"{Request.Scheme}://{Request.Host}/Admin/Settings";
            var portalUrl = await _subscriptionService.CreateBillingPortalSessionAsync(tenantId, returnUrl);
            return Redirect(portalUrl);
        }
        catch
        {
            TempData["Error"] = "Unable to open billing portal. Please ensure you have an active subscription.";
            return RedirectToPage("/Admin/Settings");
        }
    }
}
