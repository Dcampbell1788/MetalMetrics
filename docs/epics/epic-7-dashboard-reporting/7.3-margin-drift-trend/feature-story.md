# Feature 7.3 — Margin Drift Trend

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Medium

---

## User Story

**As a** company owner,
**I want** a line chart showing my actual margin percentage over time,
**so that** I can see if my quoting accuracy is improving or degrading and take corrective action.

---

## Acceptance Criteria

- [ ] Line chart displayed on the Dashboard page
- [ ] X-axis: job completion date (chronological)
- [ ] Y-axis: actual margin percentage
- [ ] Horizontal reference line at the target margin percentage (from `TenantSettings`)
- [ ] Data points represent individual completed jobs
- [ ] Trend line or moving average to show the overall direction
- [ ] Tooltips show job number, customer, and margin on hover
- [ ] Chart clearly shows when margin is above/below target
- [ ] Uses Chart.js

---

## Page Layout

```
┌─ Margin Trend Over Time ───────────────────────────────────────┐
│                                                                 │
│  30% │                              *                           │
│      │            *     *                  *                    │
│  20% │ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─ ─  Target (20%) │
│      │     *                    *                    *          │
│  10% │                *                                         │
│      │  *                                       *               │
│   0% │─────────────────────────────────────────────────────────│
│      Jan    Feb    Mar    Apr    May    Jun    Jul              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Chart type: Chart.js line chart with annotation plugin for the target line
- Chart.js annotation plugin: `chartjs-plugin-annotation` via CDN
- Data source: completed jobs ordered by `CompletedAt`, with `ActualMarginPercent`
- Target line: horizontal line at `TenantSettings.TargetMarginPercent`
- Optional: add a second dataset for a rolling average (e.g., 5-job moving average)
- Color data points: green if above target, red if below
- Consider `pointBackgroundColor` callback for per-point coloring
- If few data points, show individual markers prominently

---

## Dependencies

- Feature 7.1 (Main Dashboard)
- Feature 6.1 (Profitability Service — margin data)
- Feature 3.4 (Tenant Settings — target margin)
- Feature 5.1 (Job Actuals — actual margin values)

---

## Definition of Done

- [ ] Line chart renders with margin data over time
- [ ] Target margin line displayed
- [ ] Data points colored based on above/below target
- [ ] Tooltips show job details
- [ ] Chart handles varying numbers of data points
- [ ] Manual smoke test with multiple completed jobs
