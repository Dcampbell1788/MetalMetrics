# Feature 7.2 — Profitability Summary Chart (Estimated vs Actual)

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Complete

---

## User Story

**As a** company owner,
**I want** a bar chart showing estimated vs actual costs for completed jobs,
**so that** I can visually identify overruns and their severity.

---

## Implementation

### Chart Type

Chart.js v4 grouped bar chart via CDN. Two bars per job: Estimated (blue) + Actual (color-coded).

### Data Source

`IDashboardService.GetJobSummariesAsync(limit: 20)` returns completed jobs with estimates and actuals.

### JobSummaryDto (`Core/DTOs/JobSummaryDto.cs`)

```csharp
public class JobSummaryDto
{
    public Guid JobId { get; set; }
    public string JobNumber { get; set; }
    public string Slug { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal ActualMarginPercent { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool IsProfitable { get; set; }
    public decimal QuotePrice { get; set; }
    public string Status { get; set; }
    public decimal ActualRevenue { get; set; }
}
```

### Sorting

Data sorted **chronologically** by `CompletedAt` before rendering (client-side sort).

### Color Coding (Actual bars)

Based on overage percentage: `(ActualCost - EstimatedCost) / EstimatedCost * 100`
- **Green** (`rgba(40, 167, 69, 0.7)`) — under budget or at budget
- **Orange** (`rgba(255, 193, 7, 0.7)`) — 0-10% over budget
- **Red** (`rgba(220, 53, 69, 0.7)`) — >10% over budget

### Tooltips

Show dollar variance on hover:
```
CustomerName | +$1,234 over   (or)   -$567 under
```

### Chart.js Configuration

```javascript
var chronoJobs = jobs.slice().sort(function(a, b) {
    return new Date(a.CompletedAt) - new Date(b.CompletedAt);
});
// ... grouped bar chart with conditional backgroundColor
```

---

## Definition of Done

- [x] Grouped bar chart renders on dashboard
- [x] Sorted chronologically by completion date
- [x] 3-tier color coding (green/orange/red) for actual bars
- [x] Dollar variance in tooltips
- [x] Chart.js v4 via CDN
- [x] Limited to 20 most recent completed jobs
