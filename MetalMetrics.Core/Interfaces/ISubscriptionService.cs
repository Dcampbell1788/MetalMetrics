using MetalMetrics.Core.Entities;

namespace MetalMetrics.Core.Interfaces;

public interface ISubscriptionService
{
    Task<bool> IsTenantActiveAsync(Guid tenantId);
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string priceId, string successUrl, string cancelUrl);
    Task<string> CreateBillingPortalSessionAsync(Guid tenantId, string returnUrl);
    Task HandleStripeWebhookAsync(string json, string stripeSignature);
    Task StartTrialAsync(Guid tenantId, int trialDays = 14);
    Task<List<PlatformPlan>> GetActivePlansAsync();
}
