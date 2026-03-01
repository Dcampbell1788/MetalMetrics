using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Platform;

[Authorize(Policy = "PlatformAdmin")]
public class IndexModel : PageModel
{
    private readonly IPlatformService _platformService;

    public IndexModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    public PlatformDashboardDto Dashboard { get; set; } = new();

    public async Task OnGetAsync()
    {
        Dashboard = await _platformService.GetPlatformDashboardAsync();
    }
}
