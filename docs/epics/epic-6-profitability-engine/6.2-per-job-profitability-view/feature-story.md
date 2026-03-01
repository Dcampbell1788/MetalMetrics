# Feature 6.2 — Per-Job Profitability View

**Epic:** Epic 6 — Profitability Engine
**Status:** Complete

---

## User Story

**As a** project manager or owner,
**I want** a clear visual profitability report for each job,
**so that** I can understand job performance and identify cost overruns.

---

## Implementation

### Page: `/Jobs/Profitability/{slug}` (`Web/Pages/Jobs/Profitability/Index.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanViewReports")]`

### Layout Sections

1. **Hero Metric:** Large "PROFIT +$X,XXX" (green) or "LOSS -$X,XXX" (red) with actual margin %
2. **Warning Badges:** Bootstrap alerts for each warning from ProfitabilityService
3. **Stacked Bar Chart:** Chart.js grouped bar — Estimated vs Actual by category (Labor, Material, Machine, Overhead). Color: blue (estimated), green/red (actual based on variance direction)
4. **Detailed Breakdown Table:** Estimated, Actual, Variance ($), Variance (%) per category + totals
5. **Margin Summary:** Quote Price, Actual Revenue, Estimated Margin %, Actual Margin %, Margin Drift

### Data Flow

1. Load Job by slug (include Estimate + Actuals)
2. Call `ProfitabilityService.CalculateAsync(jobId)`
3. If null (missing data): show "Actuals not entered yet" with link to enter actuals
4. Serialize report to JSON for Chart.js

### Chart.js Bar Chart

- 4 category groups (Labor, Material, Machine, Overhead)
- 2 bars per group: Estimated (blue), Actual (green if under, red if over)
- Tooltip shows variance amount

---

## Definition of Done

- [x] Hero metric with profit/loss verdict and color coding
- [x] Warning badges for significant variances
- [x] Chart.js bar chart (estimated vs actual by category)
- [x] Detailed breakdown table
- [x] Margin summary with drift
- [x] Missing actuals handled gracefully
- [x] Role-based access enforced
