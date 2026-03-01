# SendGrid Email Setup

## Overview

MetalMetrics uses **SendGrid** for transactional email. Currently this powers the **Forgot Password** flow — when a user requests a password reset, the app sends a reset link via SendGrid.

## Prerequisites

- A SendGrid account at [sendgrid.com](https://sendgrid.com)
- Free tier supports 100 emails/day — plenty for development

## Setup Steps

### 1. Create a SendGrid account

Go to [sendgrid.com](https://sendgrid.com) and sign up for a free account.

### 2. Verify a sender identity

SendGrid requires sender verification before you can send email.

**For development** — use Single Sender Verification:
1. Go to **Settings → Sender Authentication → Single Sender Verification**
2. Click **Create a Sender**
3. Fill in the form:
   - **From Name:** `MetalMetrics`
   - **From Email Address:** a real email you own and can check (e.g., your personal or work email)
   - **Reply To:** same email
   - **Company Address:** your address (required by CAN-SPAM regulations)
   - **Nickname:** `MetalMetrics Dev` (internal label, not shown to recipients)
4. Click **Create**
5. Check your inbox for the verification email from SendGrid and click **Verify Single Sender**

> **Important:** The "From Email Address" you verify here must match the `SendGrid:SenderEmail` config value. Since `appsettings.json` defaults to `noreply@metalmetrics.app` (which you can't verify), you'll need to override it in User Secrets with the email you actually verified — see Step 4 below.

**For production** — use Domain Authentication instead of Single Sender for better deliverability and no per-address verification.

### 3. Generate an API key

1. Go to **Settings → API Keys**
2. Click **Create API Key**
   - **API Key Name:** `metalmetrics-dev`
   - **API Key Permissions:** select **Restricted Access**
3. In the permissions list, enable only **Mail Send → Full Access** (leave everything else disabled)
4. Click **Create & View**
5. Copy the key immediately — it starts with `SG.` and is only shown once

### 4. Store secrets in User Secrets

From the `MetalMetrics.Web` directory:

```bash
dotnet user-secrets set "SendGrid:ApiKey" "SG.YOUR-KEY-HERE"
dotnet user-secrets set "SendGrid:SenderEmail" "your-verified-email@example.com"
```

The `SenderEmail` override is required for local development because the default `noreply@metalmetrics.app` won't match your verified sender identity. Use the exact email you verified in Step 2.

## Configuration

The SendGrid configuration in `appsettings.json`:

```json
"SendGrid": {
  "ApiKey": "",
  "SenderEmail": "noreply@metalmetrics.app",
  "SenderName": "MetalMetrics"
}
```

| Setting | Default | Set Via |
|---------|---------|---------|
| `ApiKey` | `""` | **User Secrets** (required) |
| `SenderEmail` | `noreply@metalmetrics.app` | appsettings.json |
| `SenderName` | `MetalMetrics` | appsettings.json |

The `SenderEmail` and `SenderName` are already configured in `appsettings.json`. For development, you may want to override `SenderEmail` with your verified sender email:

```bash
dotnet user-secrets set "SendGrid:SenderEmail" "your-verified-email@example.com"
```

## Verification

1. Start the app
2. Go to the login page and click **Forgot Password**
3. Enter a demo user's email (e.g., `mike@precisionmetal.demo`)
4. Check the SendGrid **Activity Feed** in the dashboard to confirm the email was sent
5. Note: demo emails won't actually be delivered (they're fake domains), but you can verify SendGrid received the request

## Troubleshooting

### Emails not sending
- Verify the API key: `dotnet user-secrets list`
- Check that your sender identity is verified in the SendGrid dashboard
- Review app logs for SendGrid API errors (403 = sender not verified, 401 = bad key)

### "The from address does not match a verified Sender Identity"
Your `SenderEmail` in config doesn't match your verified sender. Either:
- Update the verified sender in SendGrid to match `noreply@metalmetrics.app`, or
- Override `SenderEmail` in User Secrets to match your verified sender

### Free tier limits
The free tier allows 100 emails/day. If you're testing heavily, monitor your usage in the SendGrid dashboard.

## Graceful Degradation

If the SendGrid API key is not configured:

- The app starts and runs normally
- The Forgot Password flow completes from the user's perspective (no error shown) but the reset email is never delivered
- All other features work without any impact
