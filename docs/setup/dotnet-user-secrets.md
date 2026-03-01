# .NET User Secrets Setup

## Overview

User Secrets keeps API keys and sensitive configuration **out of source control**. Instead of putting real keys in `appsettings.json`, you store them in an encrypted local file that only exists on your machine. ASP.NET Core automatically merges secrets into the configuration at runtime (Development environment only).

## Prerequisites

- .NET 8 SDK

## How It Works

The Web project is already initialized with a User Secrets ID:

```xml
<!-- MetalMetrics.Web.csproj -->
<UserSecretsId>67bc5117-1935-4dad-9480-da33ccde7927</UserSecretsId>
```

Secrets are stored at:
```
%APPDATA%\Microsoft\UserSecrets\67bc5117-1935-4dad-9480-da33ccde7927\secrets.json
```

Values in `secrets.json` override matching keys in `appsettings.json`. The `appsettings.json` file has empty string placeholders (`""`) for all keys that secrets are expected to fill.

## Setup Steps

### 1. Navigate to the Web project

All `dotnet user-secrets` commands must run from the project directory that contains the `<UserSecretsId>`:

```bash
cd MetalMetrics.Web
```

### 2. Set secrets for each service

Copy-paste the commands below, replacing the placeholder values with your real keys.

**Claude AI** (1 secret):
```bash
dotnet user-secrets set "ClaudeAI:ApiKey" "sk-ant-api03-YOUR-KEY-HERE"
```

**SendGrid** (1 secret):
```bash
dotnet user-secrets set "SendGrid:ApiKey" "SG.YOUR-KEY-HERE"
```

**Stripe** (5 secrets):
```bash
dotnet user-secrets set "Stripe:SecretKey" "sk_test_YOUR-KEY-HERE"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_YOUR-KEY-HERE"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_YOUR-SECRET-HERE"
dotnet user-secrets set "Stripe:MonthlyPriceId" "price_YOUR-MONTHLY-ID"
dotnet user-secrets set "Stripe:AnnualPriceId" "price_YOUR-ANNUAL-ID"
```

## Managing Secrets

### List all secrets
```bash
dotnet user-secrets list
```

### Remove a specific secret
```bash
dotnet user-secrets remove "ClaudeAI:ApiKey"
```

### Clear all secrets
```bash
dotnet user-secrets clear
```

## Complete Secret Reference

| Secret Key | Service | Example Value |
|------------|---------|---------------|
| `ClaudeAI:ApiKey` | Anthropic Claude | `sk-ant-api03-...` |
| `SendGrid:ApiKey` | SendGrid | `SG....` |
| `Stripe:SecretKey` | Stripe | `sk_test_...` |
| `Stripe:PublishableKey` | Stripe | `pk_test_...` |
| `Stripe:WebhookSecret` | Stripe | `whsec_...` |
| `Stripe:MonthlyPriceId` | Stripe | `price_...` |
| `Stripe:AnnualPriceId` | Stripe | `price_...` |

## Verification

After setting secrets, run:

```bash
dotnet user-secrets list
```

You should see all configured keys and their values listed.

## Troubleshooting

### "Could not find the global property 'UserSecretsId'"
You're not in the `MetalMetrics.Web` directory. Run commands from the project folder that contains the `.csproj` with `<UserSecretsId>`.

### Secrets not being picked up at runtime
- User Secrets only load in the **Development** environment. Ensure `ASPNETCORE_ENVIRONMENT=Development`.
- Restart the application after setting new secrets.

### "The configuration key 'X' has no value"
Double-check the key name matches the section structure in `appsettings.json` exactly. Keys are colon-delimited (e.g., `Stripe:SecretKey` maps to `appsettings.json` → `Stripe` → `SecretKey`).
