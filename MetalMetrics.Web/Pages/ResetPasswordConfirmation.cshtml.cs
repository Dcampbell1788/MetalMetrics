using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages;

[AllowAnonymous]
public class ResetPasswordConfirmationModel : PageModel
{
    public void OnGet() { }
}
