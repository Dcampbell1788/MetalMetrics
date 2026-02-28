using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public new int StatusCode { get; set; }
    public string ErrorMessage { get; set; } = "Error";

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet(int? statusCode = null)
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        StatusCode = statusCode ?? HttpContext.Response.StatusCode;

        ErrorMessage = StatusCode switch
        {
            404 => "Page Not Found",
            403 => "Access Denied",
            500 => "Server Error",
            _ => "Error"
        };
    }
}
