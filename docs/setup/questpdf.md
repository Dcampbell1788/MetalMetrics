# QuestPDF Setup

## Overview

MetalMetrics uses **QuestPDF** to generate PDF exports for quotes, profitability reports, and other printable documents. QuestPDF is a .NET library — it's included as a NuGet package and requires **no external account, API key, or service**.

## Prerequisites

None. QuestPDF is bundled with the project as a NuGet dependency.

## Setup Steps

There are no setup steps. QuestPDF is already:

1. **Installed** as a NuGet package in the project
2. **Licensed** in `Program.cs`:
   ```csharp
   QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
   ```

The Community license is free and fully functional.

## Configuration

No configuration needed. No User Secrets, no API keys, no external accounts.

## Verification

1. Start the app
2. Log in as any user (e.g., `mike@precisionmetal.demo` / `Demo123!`)
3. Navigate to a job's Quote View or a report page
4. Click the **Download PDF** (or similar export) button
5. A PDF file should download to your browser

## Features Powered by QuestPDF

- Quote View PDF export
- Profitability report PDF export
- Other printable report downloads

## Licensing

QuestPDF uses a tiered licensing model:

| License | Cost | Eligibility |
|---------|------|-------------|
| **Community** | Free | Companies/individuals with annual gross revenue < $1M USD |
| **Professional** | Paid | Annual gross revenue $1M–$10M USD |
| **Enterprise** | Paid | Annual gross revenue > $10M USD |

The project currently uses the **Community** license. If your organization's annual gross revenue exceeds $1M USD, you'll need to purchase a Professional or Enterprise license from [questpdf.com](https://www.questpdf.com).

## Troubleshooting

### "QuestPDF license not configured" exception
Ensure `Program.cs` contains the license setting before any PDF generation code:
```csharp
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
```

### PDF downloads return an error
- Check application logs for QuestPDF-related exceptions
- Ensure the app is running with sufficient memory (PDF generation is in-memory)

## Graceful Degradation

QuestPDF is always available — there's nothing external to fail. If the NuGet package is somehow missing, the project won't compile (build error, not a runtime issue).
