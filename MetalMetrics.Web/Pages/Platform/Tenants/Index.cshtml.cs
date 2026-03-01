using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Platform.Tenants;

[Authorize(Policy = "PlatformAdmin")]
public class IndexModel : PageModel
{
    private readonly IPlatformService _platformService;

    public IndexModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    public List<TenantSummaryDto> Tenants { get; set; } = new();

    public async Task OnGetAsync()
    {
        Tenants = await _platformService.GetAllTenantsAsync();
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid tenantId, bool enable)
    {
        await _platformService.SetTenantEnabledAsync(tenantId, enable);
        return RedirectToPage();
    }
}
