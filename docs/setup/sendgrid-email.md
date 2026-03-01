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
3. Fill in your details (use a real email you can verify)
4. Check your inbox and click the verification link

**For production** — use Domain Authentication for better deliverability.

### 3. Generate an API key

1. Go to **Settings → API Keys**
2. Click **Create API Key**
3. Name it (e.g., `metalmetrics-dev`)
4. Select **Restricted Access** and enable only **Mail Send → Full Access**
5. Click **Create & View**
6. Copy the key — it starts with `SG.`

### 4. Store the key in User Secrets

From the `MetalMetrics.Web` directory:

```bash
dotnet user-secrets set "SendGrid:ApiKey" "SG.YOUR-KEY-HERE"
```

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
