# Feature 8.3 — Error Handling & Edge Cases

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Complete

---

## User Story

**As a** user,
**I want** the application to handle errors gracefully,
**so that** I always know what happened and what to do next.

---

## Implementation

### Global Error Handler (`Web/Pages/Error.cshtml.cs`)

- `app.UseExceptionHandler("/Error")` in Program.cs (production only)
- `app.UseStatusCodePagesWithReExecute("/Error/{0}")` for automatic handling of 404, 403, 500 status codes
- Custom error page shows RequestId for debugging
- Handles status codes with appropriate messages
- Never exposes stack traces to users

### Claude API Error Handling (`Infrastructure/Services/ClaudeAIQuoteService.cs`)

| Scenario | User Message |
|----------|-------------|
| Timeout | "The AI service timed out. Try again or quote manually." |
| Rate limit (429, after retries) | "The AI service is busy. Please wait and try again." |
| Bad JSON response | "Could not parse the AI response. Try again." |
| Network failure | Catches `HttpRequestException`, returns error |

Retry logic: 2 retries on 429 with exponential backoff (2s, 4s).

Fallback: User can always use manual quote form (`/Jobs/Quote/Create/{slug}`).

### Empty States

- No jobs yet -> "No completed jobs with actuals yet" with helpful guidance
- No estimate -> links to create estimate
- No actuals -> links to enter actuals
- Dashboard with no data -> message with call to action

### Division-by-Zero Protection

All margin/variance calculations guard against zero denominators:
```csharp
var margin = revenue > 0 ? (revenue - cost) / revenue * 100 : 0;
var variance = estimated > 0 ? (actual - estimated) / estimated * 100 : 0;
```

### Duplicate Prevention

- `ActualsService.SaveAsync()` is an upsert — checks for existing actuals before insert
- Only one `JobEstimate` and one `JobActuals` per job (enforced by one-to-one relationship)

### Input Validation

- DataAnnotations on page models (`[Required]`, `[Range]`, etc.)
- jQuery unobtrusive validation for client-side
- Server-side `ModelState.IsValid` checks

---

## Definition of Done

- [x] AI failures handled with user-friendly messages + retry/fallback
- [x] Empty states with helpful messages
- [x] Division-by-zero prevented in all calculations
- [x] Duplicate data entry prevented (upsert pattern)
- [x] Global error page (404, 500)
- [x] Client + server form validation
