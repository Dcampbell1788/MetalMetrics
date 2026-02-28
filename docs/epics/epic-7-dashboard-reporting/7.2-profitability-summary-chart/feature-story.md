# Feature 7.2 — Profitability Summary Chart

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** company owner,
**I want** a bar chart showing estimated vs actual costs for all completed jobs,
**so that** I can visually identify which jobs were profitable and which lost money.

---

## Acceptance Criteria

- [ ] Bar chart displayed on the Dashboard page (below KPI cards)
- [ ] Each completed job shown as a grouped bar: Estimated Cost vs Actual Cost
- [ ] Color coding: green bars for profitable jobs, red bars for jobs at a loss
- [ ] Chart is sortable by: Date (default), Margin %, Customer Name
- [ ] Hovering/clicking a bar shows job details (tooltip or link)
- [ ] Chart handles varying numbers of jobs gracefully (scrollable if many)
- [ ] Uses Chart.js via CDN (no npm build required)
- [ ] Chart data is tenant-scoped

---

## Technical Notes

- Chart library: Chart.js v4 via CDN (`<script src="https://cdn.jsdelivr.net/npm/chart.js">`)
- Chart type: grouped bar chart (two bars per job: estimated, actual)
- Data source: `IDashboardService` returns a list of `JobSummaryDto`:
  ```csharp
  public class JobSummaryDto
  {
      public string JobNumber { get; set; }
      public string CustomerName { get; set; }
      public decimal TotalEstimatedCost { get; set; }
      public decimal TotalActualCost { get; set; }
      public decimal ActualMarginPercent { get; set; }
      public DateTime CompletedAt { get; set; }
      public bool IsProfitable { get; set; }
  }
  ```
- Serialize data to JSON and pass to Chart.js via `<script>` block
- Sorting: use JavaScript to re-render chart with sorted data, or server-side with query parameter
- Consider limiting to the most recent 20-30 jobs for readability
- Color logic: `backgroundColor` set conditionally based on `IsProfitable`

---

## Dependencies

- Feature 7.1 (Main Dashboard — chart renders on this page)
- Feature 6.1 (Profitability Service — for profit/loss data)
- Feature 3.2 (Job Entity)
- Feature 5.1 (Job Actuals)

---

## Definition of Done

- [ ] Chart renders on the dashboard with correct data
- [ ] Estimated vs Actual bars display for each completed job
- [ ] Color coding correctly indicates profit/loss
- [ ] Sorting works by date, margin, and customer
- [ ] Tooltips show job details on hover
- [ ] Chart.js loads via CDN without build tooling
- [ ] Manual smoke test with multiple jobs
