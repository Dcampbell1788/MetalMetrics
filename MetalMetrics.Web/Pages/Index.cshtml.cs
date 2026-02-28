using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            // Route to appropriate landing page based on role
            if (User.IsInRole("Journeyman") || User.IsInRole("Foreman"))
                return RedirectToPage("/Jobs/Index", new { StatusFilter = "InProgress" });
            if (User.IsInRole("Estimator"))
                return RedirectToPage("/Jobs/Index", new { StatusFilter = "Quoted" });

            return RedirectToPage("/Dashboard/Index");
        }
        return Page();
    }
}
