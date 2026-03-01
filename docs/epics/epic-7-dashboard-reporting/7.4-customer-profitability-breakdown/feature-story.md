# Feature 7.4 — Customer Profitability Breakdown

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Complete

---

## User Story

**As a** company owner,
**I want** profit/loss aggregated by customer with visual highlighting,
**so that** I can identify which customers are profitable and which are costing money.

---

## Implementation

### Data Source

`IDashboardService.GetCustomerProfitabilityAsync()` — groups all jobs with actuals by `CustomerName`, aggregates revenue/cost/margin.

### CustomerProfitabilityDto (`Core/DTOs/CustomerProfitabilityDto.cs`)

```csharp
public class CustomerProfitabilityDto
{
    public string CustomerName { get; set; }
    public int JobCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal MarginPercent { get; set; }
}
```

### Table Display

HTML table with columns: Customer, Jobs, Revenue, P/L, Margin

### Row Highlighting

Based on margin vs target:
- `table-danger` — margin below target by more than 3%
- `table-warning` — margin within 3% of target
- No class — margin at or above target

```razor
var rowClass = c.MarginPercent < Model.Kpis.TargetMarginPercent
    ? (c.MarginPercent < Model.Kpis.TargetMarginPercent - 3 ? "table-danger" : "table-warning")
    : "";
```

### Sort Toggle

Card header has two buttons: "Margin %" and "P/L" (btn-group).

Client-side JavaScript `sortCustomerTable(mode)`:
```javascript
function sortCustomerTable(mode) {
    var tbody = document.getElementById('customerTable').querySelector('tbody');
    var rows = Array.from(tbody.querySelectorAll('tr'));
    rows.sort(function(a, b) {
        if (mode === 'margin') return parseFloat(b.dataset.margin) - parseFloat(a.dataset.margin);
        else return parseFloat(b.dataset.pl) - parseFloat(a.dataset.pl);
    });
    rows.forEach(function(row) { tbody.appendChild(row); });
    // Toggle active class on buttons
}
```

Each `<tr>` has `data-margin` and `data-pl` attributes for sort values.

### Color Coding

- P/L column: `.text-profit` (green) if >= 0, `.text-loss` (red) if < 0
- Margin column: same color logic

---

## Definition of Done

- [x] Customer profitability table on dashboard
- [x] Correct aggregation across jobs
- [x] Row highlighting (red/yellow) based on target margin
- [x] Client-side sort toggle (Margin % vs P/L)
- [x] Color-coded P/L and margin columns
- [x] Data tenant-scoped
