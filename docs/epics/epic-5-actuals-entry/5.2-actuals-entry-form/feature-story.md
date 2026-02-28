# Feature 5.2 — Actuals Entry Form

**Epic:** Epic 5 — Actuals Entry (Post-Job Tracking)
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Large

---

## User Story

**As a** project manager or foreman,
**I want** to enter the actual costs for a completed job with a side-by-side comparison against the original estimate,
**so that** I can see exactly where we were over or under budget as I enter the real numbers.

---

## Acceptance Criteria

- [ ] Actuals entry page accessible at `/Jobs/{jobId}/Actuals/Enter`
- [ ] Page displays side-by-side: Estimate column (read-only) | Actuals column (editable)
- [ ] All cost fields editable: Labor Hours, Labor Rate, Material Cost, Machine Hours, Machine Rate, Overhead %
- [ ] `ActualRevenue` field for what was invoiced
- [ ] `Notes` textarea for additional context
- [ ] Real-time variance calculation as the user types (JavaScript)
- [ ] Variance color coding: green (at or under estimate), red (over estimate)
- [ ] Pre-populate actual rates with estimated rates as starting point
- [ ] Save button persists `JobActuals` record to the database
- [ ] If actuals already exist for the job, load them for editing (update, not duplicate)
- [ ] Access: Admin, Owner, ProjectManager, Foreman, Journeyman

---

## Page Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  Enter Actuals — Job #JOB-0042 (Customer: ABC Fabrication)     │
├─────────────────┬──────────────────┬────────────────────────────┤
│  Category       │  Estimated       │  Actual          Variance  │
├─────────────────┼──────────────────┼────────────────────────────┤
│  Labor Hours    │  4.5 hrs         │  [5.0    ] hrs   +0.5 🔴  │
│  Labor Rate     │  $75.00/hr       │  [$75.00 ] /hr    $0  🟢  │
│  Material Cost  │  $285.00         │  [$310.00]       +$25 🔴  │
│  Machine Hours  │  2.0 hrs         │  [1.5    ] hrs   -0.5 🟢  │
│  Machine Rate   │  $150.00/hr      │  [$150.00] /hr    $0  🟢  │
│  Overhead %     │  15%             │  [15     ] %      0%  🟢  │
├─────────────────┼──────────────────┼────────────────────────────┤
│  Total Cost     │  $987.38         │  $1,042.50       +$55 🔴  │
│  Revenue        │  $1,250 (quote)  │  [$1,250 ]                │
│  Margin         │  21.0%           │  16.6%           -4.4% 🔴 │
├─────────────────┴──────────────────┴────────────────────────────┤
│  Notes: [                                                     ] │
├─────────────────────────────────────────────────────────────────┤
│                              [Cancel]    [Save Actuals]         │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Page: `Pages/Jobs/Actuals/Enter.cshtml` + `Enter.cshtml.cs`
- Load `JobEstimate` for the side-by-side comparison
- JavaScript handles real-time variance and total calculations
- Variance = Actual - Estimated (positive = over, negative = under)
- Color thresholds: red if actual > estimated, green if actual <= estimated
- On save:
  1. Create or update `JobActuals` for the job
  2. Set `EnteredBy` to the current user
  3. Calculate `TotalActualCost`
  4. Redirect to job details or profitability view
- Service: `IActualsService.SaveActualsAsync(jobId, actualsDto)`

---

## Dependencies

- Feature 5.1 (Job Actuals Entity)
- Feature 3.3 (Quote Entity — for comparison data)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] Actuals entry page renders with side-by-side layout
- [ ] Estimated values display correctly from `JobEstimate`
- [ ] Real-time variance calculation works in the browser
- [ ] Color coding applied based on over/under estimate
- [ ] Save creates/updates `JobActuals` record
- [ ] Existing actuals load for editing
- [ ] Role-based access enforced
- [ ] At least 1 unit test for actuals service
- [ ] Manual smoke test passed
