# Feature 5.2 — Actuals Entry Form

**Epic:** Epic 5 — Actuals Entry
**Status:** Complete

---

## User Story

**As a** project manager or foreman,
**I want** to enter actual costs with a side-by-side comparison against the estimate,
**so that** I can see where we were over or under budget.

---

## Implementation

### Page: `/Jobs/Actuals/Enter/{slug}` (`Web/Pages/Jobs/Actuals/Enter.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanEnterActuals")]`

### Layout

Side-by-side display:
- **Left column:** Estimated values (read-only from JobEstimate)
- **Right column:** Actual values (editable form)
- **Variance column:** Real-time difference calculation

### Form Fields

Labor Hours, Labor Rate, Material Cost, Machine Hours, Machine Rate, Overhead %, Actual Revenue, Notes

### Pre-Population Logic

1. If existing actuals exist -> load them for editing
2. If no actuals exist -> pre-populate with estimated values as starting point

### Real-Time Variance (Client-Side JavaScript)

- Calculates `TotalActualCost` using same formula as service
- Calculates variance (Actual - Estimated) for each field
- Color coding: green if actual <= estimated, red if over
- Updates margin calculation in real-time

### OnPostAsync Flow

1. Build `JobActuals` from form values
2. Call `ActualsService.CalculateTotals(actuals)` to compute TotalActualCost
3. Set `EnteredBy` from current user email
4. Call `ActualsService.SaveAsync(actuals)` (upserts)
5. TempData success message, redirect to Job Details

---

## Definition of Done

- [x] Side-by-side layout with estimate vs actuals
- [x] Real-time variance calculation with color coding
- [x] Pre-populate from estimate or existing actuals
- [x] Upsert save (create or update)
- [x] Role-based access enforced
