# Claude AI (Anthropic) API Setup

## Overview

MetalMetrics uses the Anthropic Claude API for **AI-powered job quoting**. When an estimator creates a new quote, they can use the AI Quote feature to get an intelligent cost estimate based on job parameters. The app calls Claude via the Anthropic REST API using `IHttpClientFactory` with retry and backoff.

## Prerequisites

- An Anthropic account at [console.anthropic.com](https://console.anthropic.com)
- A funded API account (pay-per-use billing)

## Setup Steps

### 1. Create an Anthropic account

Go to [console.anthropic.com](https://console.anthropic.com) and sign up.

### 2. Add billing

Navigate to **Settings → Billing** and add a payment method. Claude API is pay-per-use.

### 3. Generate an API key

1. Go to **Settings → API Keys**
2. Click **Create Key**
3. Name it (e.g., `metalmetrics-dev`)
4. Copy the key — it starts with `sk-ant-`

### 4. Store the key in User Secrets

From the `MetalMetrics.Web` directory:

```bash
dotnet user-secrets set "ClaudeAI:ApiKey" "sk-ant-api03-YOUR-KEY-HERE"
```

## Configuration

The full Claude AI configuration in `appsettings.json`:

```json
"ClaudeAI": {
  "ApiKey": "",
  "Model": "claude-sonnet-4-6",
  "MaxTokens": 1024,
  "TimeoutSeconds": 30
}
```

Only `ApiKey` needs to be set via User Secrets. The other settings have sensible defaults:

| Setting | Default | Notes |
|---------|---------|-------|
| `Model` | `claude-sonnet-4-6` | Current model used for quoting |
| `MaxTokens` | `1024` | Max response length |
| `TimeoutSeconds` | `30` | HTTP request timeout |

## Verification

1. Start the app
2. Log in as an Estimator (e.g., `rich@precisionmetal.demo` / `Demo123!`)
3. Navigate to a job's AI Quote page
4. Submit a quote request — you should get an AI-generated estimate

## Cost

Claude API pricing is pay-per-token. A typical quote request costs approximately **$0.01–$0.05** depending on the complexity of the job description.

## Troubleshooting

### AI Quote returns an error message
- Verify the API key is set: `dotnet user-secrets list` (from `MetalMetrics.Web`)
- Check that your Anthropic account has billing configured and available credits
- Review application logs for HTTP status codes (401 = bad key, 429 = rate limited)

### Timeout errors
The default timeout is 30 seconds. If Claude is under heavy load, you may see timeout errors. These are transient — the built-in retry policy will handle most cases.

## Graceful Degradation

If the Claude AI API key is not configured:

- The app starts and runs normally
- The AI Quote feature returns a friendly error message explaining the service is unavailable
- All other features (manual quoting, job management, reports, etc.) work without any impact
