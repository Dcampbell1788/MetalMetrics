# Feature 7.5 — Estimating Accuracy (Category Variance)

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Complete

---

## User Story

**As a** company owner,
**I want** to see which cost categories I consistently underestimate or overestimate,
**so that** I can improve quoting accuracy.

---

## Implementation

### Data Source

`IDashboardService.GetCategoryVariancesAsync()` — calculates average variance (% and $) per cost category across all completed jobs with estimates and actuals.

### CategoryVarianceDto (`Core/DTOs/CategoryVarianceDto.cs`)

```csharp
public class CategoryVarianceDto
{
    public string Category { get; set; }              // "Labor", "Material", "Machine", "Overhead"
    public decimal AverageVariancePercent { get; set; }
    public string Direction { get; set; }              // "Underestimate" or "Overestimate"
    public int JobCount { get; set; }
    public decimal AverageDollarVariance { get; set; } // Avg $ impact per job
}
```

### Variance Calculation (DashboardService)

For each job, per category:
```
Percent = (Actual - Estimated) / Estimated * 100
Dollars = Actual - Estimated
```

Tracked as `List<(decimal Percent, decimal Dollars)>` tuples. Averaged across all jobs.

Labor: `(LaborHours * LaborRate)` estimated vs actual
Material: direct `MaterialCost` estimated vs actual
Machine: `(MachineHours * MachineRate)` estimated vs actual
Overhead: `Subtotal * (OverheadPercent / 100)` estimated vs actual

### Table Display

| Column | Description |
|--------|-------------|
| Category | Labor, Material, Machine, Overhead |
| Avg Variance | Percentage with +/- sign, colored red (>5%) or green (<-5%) |
| Avg $ Impact | Dollar amount formatted as currency |
| Direction | Colored arrow icon instead of text |

### Direction Icons

- **Underestimate** (positive variance, risk): Red up-arrow `&#9650;` with `.text-loss`
- **Overestimate** (negative variance, safe): Green down-arrow `&#9660;` with `.text-profit`

### Insight Alert

If worst category has >10% average variance, shows:
```
You tend to [underestimate/overestimate] [Category] by ~X%.
```

---

## Definition of Done

- [x] Category variance table on dashboard
- [x] Average variance % and $ calculated correctly
- [x] Colored arrow icons for direction
- [x] Insight alert for worst category
- [x] Color coding for significant variances
- [x] Data tenant-scoped
