# Feature 4.4 — AI Quote Review & Acceptance

**Epic:** Epic 4 — AI-Powered Quoting
**Status:** Complete

---

## User Story

**As an** estimator,
**I want** to review AI suggestions, see reasoning, and adjust values before saving,
**so that** I maintain control over the final estimate.

---

## Implementation

### Page: `/Jobs/Quote/Review/{slug}` (`Web/Pages/Jobs/Quote/Review.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanQuote")]`

### Data Flow

1. AI Quote form (4.2) stores `AIQuoteResponse` + `PromptSnapshot` in TempData as serialized JSON
2. Review page deserializes from TempData on GET
3. If TempData is empty (direct navigation), redirects to AI Quote form

### Layout

Side-by-side display:
- **Left column:** AI suggestions (read-only) — labor hrs, material cost, machine hrs, overhead %, suggested price
- **Right column:** Editable form pre-populated with AI values
- **Below:** AI reasoning text, assumptions list, confidence level badge
- **Footer:** Action buttons

### Confidence Level Display

- High: green badge
- Medium: yellow/warning badge
- Low: red badge

### Real-Time Recalculation

Client-side JavaScript recalculates `TotalEstimatedCost` and `EstimatedMarginPercent` as user edits values (same formula as QuoteService.CalculateTotals).

### OnPostAccept Flow

1. Create `JobEstimate` from edited form values
2. Call `QuoteService.CalculateTotals()` to compute cost/margin
3. Set `AIGenerated = true`
4. Set `AIPromptSnapshot` from stored snapshot
5. Set `CreatedBy` from current user email
6. Save via `QuoteService.CreateAsync()`
7. Redirect to Job Details

### Navigation Options

- **Accept & Save** -> Creates JobEstimate, redirects to Details
- **Start Over** -> Returns to AI Quote form
- **Quote Manually** -> Redirects to `/Jobs/Quote/Create/{slug}`

---

## Definition of Done

- [x] Review page shows AI suggestions + editable form
- [x] Reasoning, assumptions, confidence displayed
- [x] All fields editable with real-time recalculation
- [x] Accept creates JobEstimate with AIGenerated=true + snapshot
- [x] Start Over and Manual options work
- [x] Role-based access enforced
