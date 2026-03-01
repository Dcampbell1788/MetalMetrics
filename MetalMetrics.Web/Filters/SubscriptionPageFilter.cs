using MetalMetrics.Core.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetalMetrics.Web.Filters;

public class SubscriptionPageFilter : IAsyncPageFilter
{
    private static readonly HashSet<string> ExemptPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/Login",
        "/Register",
        "/Logout",
        "/Error",
        "/AccessDenied",
        "/SubscriptionRequired",
        "/SubscriptionExpired",
        "/ForgotPassword",
        "/ResetPassword",
        "/ResetPasswordConfirmation"
    };

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var httpContext = context.HttpContext;
        var user = httpContext.User;

        // Skip for unauthenticated users
        if (user.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        // Skip for SuperAdmin
        if (user.IsInRole("SuperAdmin"))
        {
            await next();
            return;
        }

        // Check exempt paths
        var path = context.ActionDescriptor is CompiledPageActionDescriptor page
            ? page.ViewEnginePath
            : httpContext.Request.Path.Value ?? "";

        if (ExemptPaths.Contains(path)
            || path.StartsWith("/Billing", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Platform", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/Error", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        // Check subscription status from claims (fast path)
        var statusClaim = user.FindFirst("SubscriptionStatus")?.Value;
        if (string.IsNullOrEmpty(statusClaim))
        {
            // No claim yet — allow through (legacy users before this feature)
            await next();
            return;
        }

        if (!Enum.TryParse<SubscriptionStatus>(statusClaim, out var status))
        {
            await next();
            return;
        }

        // Check tenant enabled
        var isEnabledClaim = user.FindFirst("TenantEnabled")?.Value;
        if (isEnabledClaim == "false")
        {
            context.Result = new RedirectToPageResult("/SubscriptionRequired");
            return;
        }

        switch (status)
        {
            case SubscriptionStatus.Active:
                await next();
                return;

            case SubscriptionStatus.Trial:
                // Check trial expiry from claim
                var trialEndsClaim = user.FindFirst("TrialEndsAt")?.Value;
                if (DateTime.TryParse(trialEndsClaim, out var trialEnds) && trialEnds > DateTime.UtcNow)
                {
                    await next();
                    return;
                }
                context.Result = new RedirectToPageResult("/SubscriptionExpired");
                return;

            case SubscriptionStatus.PastDue:
                // Allow with banner (banner handled in layout)
                await next();
                return;

            default:
                context.Result = new RedirectToPageResult("/SubscriptionExpired");
                return;
        }
    }
}
