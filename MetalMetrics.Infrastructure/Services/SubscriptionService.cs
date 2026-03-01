using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace MetalMetrics.Infrastructure.Services;

public class SubscriptionService : ISubscriptionService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<SubscriptionService> _logger;

    public SubscriptionService(AppDbContext db, IConfiguration config, ILogger<SubscriptionService> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<bool> IsTenantActiveAsync(Guid tenantId)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant == null || !tenant.IsEnabled) return false;

        return tenant.SubscriptionStatus switch
        {
            SubscriptionStatus.Active => true,
            SubscriptionStatus.Trial => tenant.TrialEndsAt > DateTime.UtcNow,
            SubscriptionStatus.PastDue => true, // allow with warning
            _ => false
        };
    }

    public async Task<string> CreateCheckoutSessionAsync(Guid tenantId, string priceId, string successUrl, string cancelUrl)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant not found");

        // Create Stripe Customer if needed
        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = tenant.CompanyName,
                Metadata = new Dictionary<string, string> { { "TenantId", tenantId.ToString() } }
            });
            tenant.StripeCustomerId = customer.Id;
            await _db.SaveChangesAsync();
        }

        var options = new SessionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
            {
                new() { Price = priceId, Quantity = 1 }
            },
            Mode = "subscription",
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { { "TenantId", tenantId.ToString() } }
        };

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(options);
        return session.Url;
    }

    public async Task<string> CreateBillingPortalSessionAsync(Guid tenantId, string returnUrl)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant not found");

        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new InvalidOperationException("No Stripe customer for this tenant");

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            ReturnUrl = returnUrl
        };

        var service = new Stripe.BillingPortal.SessionService();
        var session = await service.CreateAsync(options);
        return session.Url;
    }

    public async Task HandleStripeWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"];
        Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            throw;
        }

        // Idempotency check
        if (await _db.SubscriptionEvents.AnyAsync(e => e.StripeEventId == stripeEvent.Id))
        {
            _logger.LogInformation("Duplicate Stripe event {EventId}, skipping", stripeEvent.Id);
            return;
        }

        switch (stripeEvent.Type)
        {
            case "checkout.session.completed":
                await HandleCheckoutCompleted(stripeEvent);
                break;
            case "customer.subscription.updated":
                await HandleSubscriptionUpdated(stripeEvent);
                break;
            case "customer.subscription.deleted":
                await HandleSubscriptionDeleted(stripeEvent);
                break;
            case "invoice.payment_succeeded":
                await HandlePaymentSucceeded(stripeEvent);
                break;
            case "invoice.payment_failed":
                await HandlePaymentFailed(stripeEvent);
                break;
        }

        _db.SubscriptionEvents.Add(new SubscriptionEvent
        {
            EventType = stripeEvent.Type,
            StripeEventId = stripeEvent.Id,
            Details = json.Length > 4000 ? json[..4000] : json,
            OccurredAt = DateTime.UtcNow,
            TenantId = Guid.Empty // will be set per event where possible
        });
        await _db.SaveChangesAsync();
    }

    public async Task StartTrialAsync(Guid tenantId, int trialDays = 14)
    {
        var tenant = await _db.Tenants.FindAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant not found");

        tenant.SubscriptionStatus = SubscriptionStatus.Trial;
        tenant.TrialEndsAt = DateTime.UtcNow.AddDays(trialDays);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PlatformPlan>> GetActivePlansAsync()
    {
        return await _db.PlatformPlans.Where(p => p.IsActive).ToListAsync();
    }

    private async Task HandleCheckoutCompleted(Event stripeEvent)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session == null) return;

        var tenantIdStr = session.Metadata?.GetValueOrDefault("TenantId");
        if (!Guid.TryParse(tenantIdStr, out var tenantId)) return;

        var tenant = await _db.Tenants.FindAsync(tenantId);
        if (tenant == null) return;

        tenant.StripeSubscriptionId = session.SubscriptionId;
        tenant.SubscriptionStatus = SubscriptionStatus.Active;
        tenant.SubscriptionStartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private async Task HandleSubscriptionUpdated(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == subscription.CustomerId);
        if (tenant == null) return;

        tenant.StripeSubscriptionId = subscription.Id;
        tenant.SubscriptionEndsAt = subscription.CurrentPeriodEnd;

        if (subscription.Items?.Data?.Count > 0)
        {
            tenant.StripePriceId = subscription.Items.Data[0].Price?.Id;
        }

        tenant.SubscriptionStatus = subscription.Status switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Cancelled,
            "trialing" => SubscriptionStatus.Trial,
            _ => tenant.SubscriptionStatus
        };

        await _db.SaveChangesAsync();
    }

    private async Task HandleSubscriptionDeleted(Event stripeEvent)
    {
        var subscription = stripeEvent.Data.Object as Subscription;
        if (subscription == null) return;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == subscription.CustomerId);
        if (tenant == null) return;

        tenant.SubscriptionStatus = SubscriptionStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    private async Task HandlePaymentSucceeded(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == invoice.CustomerId);
        if (tenant == null) return;

        tenant.SubscriptionStatus = SubscriptionStatus.Active;
        await _db.SaveChangesAsync();
    }

    private async Task HandlePaymentFailed(Event stripeEvent)
    {
        var invoice = stripeEvent.Data.Object as Invoice;
        if (invoice == null) return;

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == invoice.CustomerId);
        if (tenant == null) return;

        tenant.SubscriptionStatus = SubscriptionStatus.PastDue;
        await _db.SaveChangesAsync();
    }
}
