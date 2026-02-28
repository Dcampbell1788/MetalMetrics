# Feature 6.3 — Margin Threshold Alerts

**Epic:** Epic 6 — Profitability Engine & Per-Job Analysis
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Small

---

## User Story

**As a** company owner,
**I want** to set a target margin percentage and see alerts when jobs fall below that threshold,
**so that** I can quickly identify underperforming jobs and take corrective action.

---

## Acceptance Criteria

- [ ] `TargetMarginPercent` configurable in `TenantSettings` (default: 20%)
- [ ] Settings page (Feature 3.4) includes the target margin field
- [ ] Jobs where actual margin falls below the target are flagged
- [ ] Visual warning indicator on the Job List page for flagged jobs
- [ ] Warning badge on the per-job profitability view (Feature 6.2)
- [ ] Flag is based on actual margin when actuals exist, otherwise not flagged
- [ ] Dashboard (Feature 7.1) shows a count of jobs below target margin

---

## Technical Notes

- `TargetMarginPercent` is already part of the `TenantSettings` entity (Feature 3.4)
- Alert logic lives in `ProfitabilityService`:
  ```csharp
  if (report.ActualMarginPercent < tenantSettings.TargetMarginPercent)
  {
      report.Warnings.Add($"Margin ({report.ActualMarginPercent:F1}%) is below target ({tenantSettings.TargetMarginPercent:F1}%)");
  }
  ```
- Job List visual indicator: add a warning icon or colored badge next to flagged jobs
- Consider a CSS class like `.margin-warning` for consistent styling
- Query for "jobs below target": filter completed jobs where actual margin < target
- This feature enhances existing pages rather than creating new ones

---

## Dependencies

- Feature 3.4 (Tenant Settings — `TargetMarginPercent` field)
- Feature 6.1 (Profitability Calculation Service — warning generation)
- Feature 6.2 (Per-Job Profitability View — warning display)
- Feature 3.2 (Job List — visual indicator)

---

## Definition of Done

- [ ] `TargetMarginPercent` editable in tenant settings
- [ ] Jobs below target margin show warning indicator on job list
- [ ] Warning appears in per-job profitability view
- [ ] Default value (20%) applied for new tenants
- [ ] At least 1 unit test for threshold comparison logic
- [ ] Manual smoke test with a job above and below threshold
