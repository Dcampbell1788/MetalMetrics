using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Platform.Tenants;

[Authorize(Policy = "PlatformAdmin")]
public class DetailsModel : PageModel
{
    private readonly IPlatformService _platformService;

    public DetailsModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    public TenantDetailDto Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var detail = await _platformService.GetTenantDetailAsync(id);
        if (detail == null) return NotFound();
        Tenant = detail;
        return Page();
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid tenantId, bool enable)
    {
        await _platformService.SetTenantEnabledAsync(tenantId, enable);
        return RedirectToPage(new { id = tenantId });
    }
}
