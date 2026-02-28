# Feature 6.1 — Profitability Calculation Service

**Epic:** Epic 6 — Profitability Engine & Per-Job Analysis
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Medium

---

## User Story

**As a** developer,
**I want** a service that calculates detailed profitability metrics by comparing estimates to actuals,
**so that** the application can generate accurate profit/loss reports broken down by cost category.

---

## Acceptance Criteria

- [ ] `IProfitabilityService` interface defined in `MetalMetrics.Core`
- [ ] `ProfitabilityService` implementation created
- [ ] Calculates variance ($ and %) for each cost category: Labor, Material, Machine, Overhead
- [ ] Calculates `TotalEstimatedCost` and `TotalActualCost`
- [ ] Calculates `EstimatedMargin` and `ActualMargin` ($ and %)
- [ ] Calculates `MarginDrift` (difference between estimated and actual margin)
- [ ] Returns `OverallVerdict`: "Profit", "Loss", or "Break Even"
- [ ] Generates warning messages for significant variances (e.g., > 20% over estimate)
- [ ] Handles edge cases: zero costs, missing actuals, division by zero
- [ ] Service registered in DI container

---

## Output DTO

```csharp
public class JobProfitabilityReport
{
    // Per-category variance
    public VarianceDetail LaborVariance { get; set; }
    public VarianceDetail MaterialVariance { get; set; }
    public VarianceDetail MachineVariance { get; set; }
    public VarianceDetail OverheadVariance { get; set; }

    // Totals
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal QuotedPrice { get; set; }
    public decimal ActualRevenue { get; set; }

    // Margins
    public decimal EstimatedMarginDollars { get; set; }
    public decimal EstimatedMarginPercent { get; set; }
    public decimal ActualMarginDollars { get; set; }
    public decimal ActualMarginPercent { get; set; }
    public decimal MarginDriftDollars { get; set; }
    public decimal MarginDriftPercent { get; set; }

    // Verdict
    public string OverallVerdict { get; set; } // "Profit" | "Loss" | "Break Even"
    public List<string> Warnings { get; set; }
}

public class VarianceDetail
{
    public decimal EstimatedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal VarianceDollars { get; set; }
    public decimal VariancePercent { get; set; }
}
```

---

## Technical Notes

- Service: `MetalMetrics.Core/Services/ProfitabilityService.cs`
- Method signature: `Task<JobProfitabilityReport> CalculateAsync(Guid jobId)`
- Variance calculation: `Actual - Estimated` (positive = over budget)
- Variance %: `(Actual - Estimated) / Estimated * 100` (guard against divide by zero)
- Margin: `(Revenue - Cost) / Revenue * 100`
- Verdict: `ActualRevenue > TotalActualCost` = Profit, equal = Break Even, less = Loss
- Warning thresholds (configurable via `TenantSettings.TargetMarginPercent`):
  - Category variance > 20%: "Material cost exceeded estimate by X%"
  - Actual margin below target: "Margin (X%) is below target (Y%)"
  - Margin drift > 10 points: "Significant margin drift detected"

---

## Dependencies

- Feature 3.3 (Quote Entity — estimated values)
- Feature 5.1 (Job Actuals Entity — actual values)
- Feature 3.4 (Tenant Settings — for target margin threshold)

---

## Definition of Done

- [ ] `IProfitabilityService` interface and implementation created
- [ ] All variance calculations correct
- [ ] Margin calculations correct
- [ ] Verdict logic correct
- [ ] Warnings generated for significant variances
- [ ] Edge cases handled (zero values, missing data)
- [ ] Service registered in DI
- [ ] At least 3 unit tests covering profit, loss, and break-even scenarios
