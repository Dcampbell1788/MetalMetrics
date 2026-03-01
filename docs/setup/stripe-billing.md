# Stripe Billing Setup

## Overview

MetalMetrics uses **Stripe** for subscription billing. Tenants subscribe to a plan (monthly or annual) to access the platform. Stripe powers:

- **Checkout** — new subscription purchase flow
- **Customer Portal** — self-service plan management (upgrade, downgrade, cancel)
- **Subscription Enforcement** — the `SubscriptionPageFilter` gates tenant access based on subscription status
- **Webhooks** — Stripe notifies the app of subscription lifecycle events

## Prerequisites

- A Stripe account at [stripe.com](https://stripe.com)
- No payment needed — use **Test Mode** for development

## Setup Steps

### 1. Create a Stripe account

Go to [stripe.com](https://stripe.com) and sign up. You'll land on the dashboard in **Test Mode** by default (toggle in the top-right).

### 2. Copy your API keys

1. Go to **Developers → API Keys**
2. Copy the **Publishable key** (`pk_test_...`)
3. Copy the **Secret key** (`sk_test_...`) — click "Reveal test key" first

### 3. Create a product with two prices

1. Go to **Products → Add Product**
2. Name: `MetalMetrics Pro` (or any name)
3. Add two **Recurring** prices:
   - **Monthly**: $99.00 / month → after creating, copy the Price ID (`price_...`)
   - **Annual**: $990.00 / year → after creating, copy the Price ID (`price_...`)

### 4. Enable the Customer Portal

1. Go to **Settings → Billing → Customer Portal**
2. Enable the portal
3. Configure allowed actions (cancel subscription, switch plans, update payment method)
4. Save

### 5. Set up webhooks

#### For deployed environments:

1. Go to **Developers → Webhooks → Add Endpoint**
2. Endpoint URL: `https://your-domain/api/stripe/webhook`
3. Select these events:
   - `checkout.session.completed`
   - `customer.subscription.updated`
   - `customer.subscription.deleted`
   - `invoice.payment_succeeded`
   - `invoice.payment_failed`
4. Click **Add Endpoint**
5. Copy the **Signing secret** (`whsec_...`)

#### For local development:

Install the Stripe CLI and forward events to your local app:

```bash
# Install Stripe CLI (Windows — via scoop)
scoop install stripe

# Or download from https://stripe.com/docs/stripe-cli

# Login
stripe login

# Forward webhooks to your local app
stripe listen --forward-to https://localhost:7001/api/stripe/webhook
```

The CLI prints a webhook signing secret (`whsec_...`) — use that as your `Stripe:WebhookSecret`.

### 6. Store all secrets in User Secrets

From the `MetalMetrics.Web` directory:

```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR-SECRET-KEY"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR-PUBLISHABLE-KEY"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR-WEBHOOK-SECRET"
dotnet user-secrets set "Stripe:MonthlyPriceId" "price_YOUR-MONTHLY-PRICE-ID"
dotnet user-secrets set "Stripe:AnnualPriceId" "price_YOUR-ANNUAL-PRICE-ID"
```

## Configuration

The Stripe configuration in `appsettings.json`:

```json
"Stripe": {
  "SecretKey": "",
  "PublishableKey": "",
  "WebhookSecret": "",
  "MonthlyPriceId": "",
  "AnnualPriceId": ""
}
```

All five values must be set via User Secrets:

| Secret Key | Source | Example |
|------------|--------|---------|
| `Stripe:SecretKey` | Developers → API Keys | `sk_test_51...` |
| `Stripe:PublishableKey` | Developers → API Keys | `pk_test_51...` |
| `Stripe:WebhookSecret` | Webhook endpoint or Stripe CLI | `whsec_...` |
| `Stripe:MonthlyPriceId` | Product → Monthly price | `price_1...` |
| `Stripe:AnnualPriceId` | Product → Annual price | `price_1...` |

## Verification

### Test checkout flow

1. Start the app
2. Log in as a tenant Owner (e.g., `mike@precisionmetal.demo` / `Demo123!`)
3. Navigate to the subscription/checkout page
4. Use Stripe's test card: `4242 4242 4242 4242`, any future expiry, any CVC
5. Complete checkout — subscription should activate

### Test webhook delivery

If using the Stripe CLI locally:

```bash
stripe trigger checkout.session.completed
```

Check your app logs for webhook processing messages.

### Test the Customer Portal

After a subscription is active, navigate to the billing portal page. It should redirect to Stripe's hosted portal where the user can manage their subscription.

## Troubleshooting

### Checkout page errors
- Verify all 5 Stripe secrets are set: `dotnet user-secrets list`
- Ensure Price IDs match actual prices in your Stripe dashboard
- Check that you're using **Test Mode** keys (prefixed `sk_test_` / `pk_test_`)

### Webhooks not arriving
- **Local dev**: ensure `stripe listen` is running and the forwarding URL matches your app's port
- **Deployed**: check the webhook endpoint URL and that all required events are selected
- Review the **Developers → Webhooks → [your endpoint] → Attempts** tab for delivery logs

### "No such price" error
The Price IDs in your User Secrets don't match any price in your Stripe account. Go to **Products** in the Stripe dashboard, click into your product, and copy the correct `price_` IDs.

### Subscription status not updating
Webhooks may not be reaching the app. Check:
1. Stripe CLI is running (local dev)
2. Webhook signing secret matches
3. App logs for signature verification errors

## Graceful Degradation

If Stripe is not configured:

- The app starts and runs normally
- Demo tenants show as **Active** (their subscription status is seeded in the database)
- Checkout and billing portal pages will error when attempting to create Stripe sessions
- The `SubscriptionPageFilter` still enforces access based on the tenant's stored subscription status
- New tenants created without Stripe won't have an active subscription and will be gated
