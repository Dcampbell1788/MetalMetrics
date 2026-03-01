using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages.Billing;

[Authorize]
public class SuccessModel : PageModel
{
    public void OnGet() { }
}
