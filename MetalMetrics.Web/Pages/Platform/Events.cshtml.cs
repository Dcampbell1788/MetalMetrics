using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Platform;

[Authorize(Policy = "PlatformAdmin")]
public class EventsModel : PageModel
{
    private readonly IPlatformService _platformService;

    public EventsModel(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    public List<SubscriptionEvent> Events { get; set; } = new();

    public async Task OnGetAsync()
    {
        Events = await _platformService.GetSubscriptionEventsAsync(limit: 100);
    }
}
