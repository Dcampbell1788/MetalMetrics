# Feature 7.5 — Category Variance Heatmap

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Pending
**Priority:** Low (Nice to Have)
**Estimated Effort:** Medium

---

## User Story

**As a** company owner,
**I want** to see which cost categories I consistently underestimate or overestimate across all jobs,
**so that** I can improve my quoting accuracy by knowing where my estimates are weakest.

---

## Acceptance Criteria

- [ ] Heatmap or summary chart displayed on the Dashboard
- [ ] Shows average variance (%) for each cost category: Labor, Material, Machine, Overhead
- [ ] Visual intensity or color indicates the magnitude of the variance
- [ ] Positive variance (underestimated) shown in red
- [ ] Negative variance (overestimated) shown in blue or green
- [ ] Based on aggregated data across all completed jobs with actuals
- [ ] Answers: "Which cost category do you consistently underestimate?"
- [ ] Data is tenant-scoped

---

## Page Layout

```
┌─ Estimating Accuracy by Category ──────────────────────────────┐
│                                                                 │
│  Category   │ Avg Variance │ Direction      │ Accuracy          │
│  ───────────│──────────────│────────────────│───────────────────│
│  Labor      │    +12.3%    │ Underestimate  │ ██████████░░ 🔴  │
│  Material   │    +8.1%     │ Underestimate  │ ████████░░░░ 🟡  │
│  Machine    │    -3.2%     │ Overestimate   │ ███░░░░░░░░░ 🟢  │
│  Overhead   │    +5.4%     │ Underestimate  │ █████░░░░░░░ 🟡  │
│                                                                 │
│  💡 Insight: You tend to underestimate labor by ~12%.           │
│     Consider adding a 10-15% buffer to labor estimates.         │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Data source: average `VariancePercent` per category across all completed jobs
- Service method: `IDashboardService.GetCategoryVarianceAsync()`
- DTO:
  ```csharp
  public class CategoryVarianceDto
  {
      public string Category { get; set; } // "Labor", "Material", "Machine", "Overhead"
      public decimal AverageVariancePercent { get; set; }
      public string Direction { get; set; } // "Underestimate" or "Overestimate"
      public int JobCount { get; set; } // Number of jobs in the average
  }
  ```
- Calculation for each category:
  ```
  AVG((ActualValue - EstimatedValue) / EstimatedValue * 100) across all completed jobs
  ```
- Visual options:
  - Simple table with color-coded bars (easiest for hackathon)
  - Chart.js radar chart or horizontal bar chart
  - True heatmap (more complex, lower priority)
- Consider adding a text insight: identify the worst category and suggest improvement
- Guard against division by zero (skip jobs where estimated value is zero)

---

## Dependencies

- Feature 7.1 (Main Dashboard)
- Feature 3.3 (Quote Entity — estimated values)
- Feature 5.1 (Job Actuals — actual values)
- Feature 6.1 (Profitability Service — variance calculations)

---

## Definition of Done

- [ ] Category variance data displayed on dashboard
- [ ] Average variance calculated correctly across all jobs
- [ ] Visual indicator shows variance magnitude and direction
- [ ] Color coding consistent (red = underestimate, green = overestimate)
- [ ] Data is tenant-scoped
- [ ] Manual smoke test with varied job data
