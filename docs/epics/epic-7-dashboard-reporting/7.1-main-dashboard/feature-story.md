# Feature 7.1 — Main Dashboard

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Complete

---

## User Story

**As a** company owner or project manager,
**I want** key business metrics at a glance when I log in,
**so that** I can quickly assess business health.

---

## Implementation

### Page: `/Dashboard` (`Web/Pages/Dashboard/Index.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanViewReports")]`
**Landing page** for Owner/Admin/ProjectManager (redirected from Index.cshtml.cs).

### IDashboardService (`Core/Interfaces/IDashboardService.cs`)

```csharp
Task<DashboardKpiDto> GetKpisAsync();
Task<List<JobSummaryDto>> GetJobSummariesAsync(int limit = 20);
Task<List<CustomerProfitabilityDto>> GetCustomerProfitabilityAsync();
Task<List<CategoryVarianceDto>> GetCategoryVariancesAsync();
Task<List<AtRiskJobDto>> GetAtRiskJobsAsync(decimal thresholdPercent = 10);
```

### DashboardKpiDto (`Core/DTOs/DashboardKpiDto.cs`)

```csharp
public class DashboardKpiDto
{
    public int TotalJobs { get; set; }
    public int JobsThisMonth { get; set; }
    public decimal AverageMarginPercent { get; set; }
    public int JobsOverBudget { get; set; }          // Below target margin
    public decimal RevenueThisMonth { get; set; }
    public decimal TargetMarginPercent { get; set; }
    public decimal TotalRevenue { get; set; }          // All-time revenue
    public int InProgressCount { get; set; }
    public int QuotedCount { get; set; }
    public decimal InProgressEstimatedValue { get; set; }
}
```

### Page Model Properties

```csharp
public DashboardKpiDto Kpis { get; set; }
public List<JobSummaryDto> JobSummaries { get; set; }
public List<CustomerProfitabilityDto> CustomerProfitability { get; set; }
public List<CategoryVarianceDto> CategoryVariances { get; set; }
public List<AtRiskJobDto> AtRiskJobs { get; set; }
public string JobSummariesJson { get; set; }  // Serialized for Chart.js
```

### Dashboard Sections (top to bottom)

1. **At-Risk Alert Banner** — Red alerts for InProgress jobs >10% over budget
2. **KPI Cards** (5 cards, `row-cols-lg-5`) — Total Jobs, Avg Margin, Below Target (clickable), Revenue (Month), Total Revenue
3. **Work Pipeline** — Quoted count, InProgress count (both clickable), InProgress estimated value
4. **Est vs Actual Chart** — Chronologically sorted, color-coded bars
5. **Margin Trend Chart** — Line chart with target annotation line
6. **Estimating Accuracy Table** — Variance % + $ + arrow icons
7. **Customer Profitability Table** — Row highlighting + sort toggle

### Empty State

If no completed jobs with actuals, shows message: "No completed jobs with actuals yet." Pipeline and KPI sections still show.

---

## Definition of Done

- [x] Dashboard is post-login landing page for Owner/Admin/PM
- [x] 5 KPI cards with correct data
- [x] At-risk alert banner
- [x] Work pipeline section with clickable links
- [x] 2 charts (est vs actual, margin trend)
- [x] 2 tables (estimating accuracy, customer profitability)
- [x] Data tenant-scoped
- [x] Empty state handled
