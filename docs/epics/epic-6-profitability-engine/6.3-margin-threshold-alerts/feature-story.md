# Feature 6.3 — Margin Threshold Alerts

**Epic:** Epic 6 — Profitability Engine
**Status:** Complete

---

## User Story

**As a** company owner,
**I want** a configurable target margin with alerts when jobs fall below it,
**so that** I can identify underperforming jobs.

---

## Implementation

### TargetMarginPercent

Stored in `TenantSettings.TargetMarginPercent`. Editable at `/Admin/Settings`.

Default values: 25% (Precision Metal Works), 15% (Budget Fabricators), 20% (new registrations).

### Where Threshold Alerts Appear

1. **ProfitabilityService warnings:** "Margin (X%) is below target (Y%)" added to report warnings
2. **Job List (`/Jobs`):** `TargetMarginPercent` loaded and available for comparison in the view
3. **Dashboard KPI card:** "Below Target Margin" count — shows number of completed jobs with actuals where actual margin < target
4. **Dashboard "Below Target" link:** Clickable KPI card navigates to `/Jobs?special=BelowTarget` which filters to only those jobs
5. **Dashboard At-Risk banner:** InProgress jobs with actuals >10% over estimated cost show as at-risk alerts
6. **Customer Profitability table:** Rows highlighted `table-danger` (below target) or `table-warning` (within 3% of target)

### BelowTarget Special Filter

`Jobs/Index.cshtml.cs` supports `?special=BelowTarget` query string which filters to jobs where:
```csharp
j.Actuals != null && j.Actuals.ActualRevenue > 0 &&
(j.Actuals.ActualRevenue - j.Actuals.TotalActualCost) / j.Actuals.ActualRevenue * 100 < TargetMarginPercent
```

---

## Definition of Done

- [x] TargetMarginPercent configurable in TenantSettings
- [x] ProfitabilityService generates margin warning
- [x] Dashboard KPI count of jobs below target
- [x] Clickable "Below Target" card links to filtered job list
- [x] At-risk alert banner for InProgress jobs over budget
- [x] Customer table row highlighting
- [x] 1 unit test for below-target-margin warning
