# Feature 6.2 — Per-Job Profitability View

**Epic:** Epic 6 — Profitability Engine & Per-Job Analysis
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Large

---

## User Story

**As a** project manager or owner,
**I want** a clear, visual profitability report for each job showing whether we made or lost money,
**so that** I can quickly understand job performance and identify which cost categories were over or under budget.

---

## Acceptance Criteria

- [ ] Profitability page accessible at `/Jobs/{jobId}/Profitability`
- [ ] Hero metric at the top: "PROFIT +$X,XXX" (green) or "LOSS -$X,XXX" (red)
- [ ] Stacked bar chart comparing Estimated vs Actual costs by category
- [ ] Red/green color coding for each category variance
- [ ] Margin metrics displayed: Estimated Margin %, Actual Margin %, Margin Drift
- [ ] Warning badges for significant variances (from profitability service)
- [ ] Table with detailed numbers: Estimated, Actual, Variance ($), Variance (%) per category
- [ ] Link back to job details
- [ ] Page shows "Actuals not entered" message if `JobActuals` doesn't exist
- [ ] Access: Admin, Owner, ProjectManager

---

## Page Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  Job #JOB-0042 — ABC Fabrication                               │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │          ✅ PROFIT  +$207.62                            │    │
│  │          Actual Margin: 16.6%  (Target: 20%)            │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ⚠️ Material cost exceeded estimate by 8.8%                    │
│  ⚠️ Margin (16.6%) is below target (20%)                      │
│                                                                 │
│  ┌─ Estimated vs Actual Cost by Category ──────────────────┐   │
│  │  Labor    ████████████ $337  ███████████████ $375  🔴   │   │
│  │  Material ████████████ $285  █████████████████ $310  🔴 │   │
│  │  Machine  ████████████ $300  █████████████ $225  🟢     │   │
│  │  Overhead ████████████ $65   ████████████████ $82  🔴   │   │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  ┌─ Detailed Breakdown ───────────────────────────────────┐    │
│  │  Category  │ Estimated │ Actual  │ Var ($) │ Var (%)   │    │
│  │  Labor     │ $337.50   │ $375.00 │ +$37.50 │ +11.1%   │    │
│  │  Material  │ $285.00   │ $310.00 │ +$25.00 │ +8.8%    │    │
│  │  Machine   │ $300.00   │ $225.00 │ -$75.00 │ -25.0%   │    │
│  │  Overhead  │ $65.14    │ $82.50  │ +$17.36 │ +26.6%   │    │
│  │  TOTAL     │ $987.64   │ $992.50 │ +$4.86  │ +0.5%    │    │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
│  Quote Price: $1,250.00    Actual Revenue: $1,200.12            │
│  Est. Margin: 21.0%       Actual Margin: 16.6%                 │
│  Margin Drift: -4.4 points                                     │
│                                                                 │
│  [← Back to Job]                                               │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Page: `Pages/Jobs/Profitability/Index.cshtml` + `Index.cshtml.cs`
- Calls `IProfitabilityService.CalculateAsync(jobId)` to get the report
- Chart: use Chart.js (CDN) for the stacked bar chart
  - Two bars per category: blue (estimated), actual color based on variance (green under, red over)
- Hero metric: large text with conditional CSS class (profit-green / loss-red)
- Warning badges: use Bootstrap alerts or custom badges
- Handle missing actuals: display a message with a link to enter actuals

---

## Dependencies

- Feature 6.1 (Profitability Calculation Service)
- Feature 3.3 (Quote Entity)
- Feature 5.1 (Job Actuals Entity)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] Profitability page renders with all sections
- [ ] Hero metric displays correct verdict with color coding
- [ ] Bar chart renders with Chart.js
- [ ] Detailed table shows correct numbers
- [ ] Warning badges display for significant variances
- [ ] Missing actuals handled gracefully
- [ ] Role-based access enforced
- [ ] Manual smoke test with profit and loss scenarios
