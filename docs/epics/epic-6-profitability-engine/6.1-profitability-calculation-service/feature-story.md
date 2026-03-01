# Feature 6.1 — Profitability Calculation Service

**Epic:** Epic 6 — Profitability Engine
**Status:** Complete

---

## User Story

**As a** developer,
**I want** a service that calculates detailed profitability by comparing estimates to actuals,
**so that** the application can generate accurate profit/loss reports by cost category.

---

## Implementation

### IProfitabilityService (`Core/Interfaces/IProfitabilityService.cs`)

```csharp
Task<JobProfitabilityReport?> CalculateAsync(Guid jobId);
```

Returns null if job not found or missing estimate/actuals.

### ProfitabilityService (`Infrastructure/Services/ProfitabilityService.cs`)

**Per-Category Variance Calculation:**
For each category (Labor, Material, Machine, Overhead):
```
VarianceDollars = ActualAmount - EstimatedAmount
VariancePercent = EstimatedAmount > 0 ? VarianceDollars / EstimatedAmount * 100 : 0
```

**Category Amounts:**
- Labor: `Hours * Rate`
- Material: direct cost
- Machine: `Hours * Rate`
- Overhead: `Subtotal * (OverheadPercent / 100)`

**Margin Calculations:**
```
EstimatedMarginDollars = QuotePrice - TotalEstimatedCost
EstimatedMarginPercent = QuotePrice > 0 ? (QuotePrice - TotalEstimatedCost) / QuotePrice * 100 : 0
ActualMarginDollars = ActualRevenue - TotalActualCost
ActualMarginPercent = ActualRevenue > 0 ? (ActualRevenue - TotalActualCost) / ActualRevenue * 100 : 0
MarginDriftDollars = ActualMarginDollars - EstimatedMarginDollars
MarginDriftPercent = ActualMarginPercent - EstimatedMarginPercent
```

**Verdict:**
- `ActualMarginDollars > 0` -> "Profit"
- `ActualMarginDollars < 0` -> "Loss"
- `ActualMarginDollars == 0` -> "Break Even"

**Warning Generation:**
- Category variance > 20%: "X cost exceeded estimate by Y%"
- Actual margin < TargetMarginPercent: "Margin (X%) is below target (Y%)"
- Margin drift > 10 points: "Significant margin drift of X%"

### DTOs

**JobProfitabilityReport (`Core/DTOs/JobProfitabilityReport.cs`):**
- `LaborVariance`, `MaterialVariance`, `MachineVariance`, `OverheadVariance` (each is `VarianceDetail`)
- `TotalEstimatedCost`, `TotalActualCost`, `QuotedPrice`, `ActualRevenue`
- `EstimatedMarginDollars/Percent`, `ActualMarginDollars/Percent`
- `MarginDriftDollars/Percent`
- `OverallVerdict` (string), `Warnings` (List<string>)

**VarianceDetail (`Core/DTOs/VarianceDetail.cs`):**
- `EstimatedAmount`, `ActualAmount`, `VarianceDollars`, `VariancePercent`

---

## Definition of Done

- [x] IProfitabilityService with CalculateAsync
- [x] Per-category variance ($ and %) calculations
- [x] Margin calculations with drift tracking
- [x] Verdict logic (Profit/Loss/Break Even)
- [x] Warning generation for significant variances
- [x] Edge cases handled (zero values, missing data)
- [x] 5 unit tests (profit, loss, break-even, category warning, margin warning)
