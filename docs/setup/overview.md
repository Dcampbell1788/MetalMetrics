# External Dependencies Setup — Overview

MetalMetrics relies on five external services. This guide explains what each does, whether it's required, and where to find the detailed setup instructions.

## Dependency Matrix

| Service | Required? | Powers | Config Section | Setup Guide |
|---------|-----------|--------|----------------|-------------|
| SQL Server LocalDB | **Yes** | All data storage, Identity | `ConnectionStrings:DefaultConnection` | [sql-server-localdb.md](sql-server-localdb.md) |
| .NET User Secrets | **Yes** (for any API key) | Secure key storage | N/A (built-in) | [dotnet-user-secrets.md](dotnet-user-secrets.md) |
| Claude AI (Anthropic) | No | AI-powered quoting | `ClaudeAI:ApiKey` | [claude-ai-api.md](claude-ai-api.md) |
| SendGrid | No | Transactional email | `SendGrid:ApiKey` | [sendgrid-email.md](sendgrid-email.md) |
| Stripe | No | Subscription billing | `Stripe:*` (5 keys) | [stripe-billing.md](stripe-billing.md) |
| QuestPDF | No (bundled) | PDF export | None needed | [questpdf.md](questpdf.md) |

## Minimum Setup to Run the App

You only need two things to launch MetalMetrics locally:

1. **SQL Server LocalDB** — ships with Visual Studio. [Setup guide](sql-server-localdb.md)
2. **Run EF migrations** — creates the database and seeds demo data.

That's it. All other services degrade gracefully.

## Graceful Degradation Summary

| Service Not Configured | What Happens |
|------------------------|--------------|
| Claude AI | AI Quote page shows a friendly error message. All other features work normally. |
| SendGrid | App runs fine. Forgot Password flow silently fails to deliver the reset email. |
| Stripe | App boots. Demo tenants display as Active (seeded). Checkout and billing portal pages return errors. |
| QuestPDF | Always available — no external account needed. Bundled NuGet package with Community license. |

## Recommended Setup Order

1. [SQL Server LocalDB](sql-server-localdb.md) — get the app running
2. [.NET User Secrets](dotnet-user-secrets.md) — learn how secrets work
3. [Claude AI](claude-ai-api.md) — quick win, one key
4. [SendGrid](sendgrid-email.md) — enables password reset
5. [Stripe](stripe-billing.md) — most involved setup
6. [QuestPDF](questpdf.md) — nothing to configure, just read the licensing note
