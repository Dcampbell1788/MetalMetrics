# Feature 7.4 — Customer Profitability Breakdown

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Medium

---

## User Story

**As a** company owner,
**I want** to see profit and loss aggregated by customer,
**so that** I can identify which customers are consistently profitable and which ones are costing me money.

---

## Acceptance Criteria

- [ ] Customer profitability table or chart displayed on the Dashboard
- [ ] Aggregates all completed jobs per customer
- [ ] Shows per customer: Total Revenue, Total Cost, Profit/Loss ($), Margin (%), Job Count
- [ ] Sortable by: Customer Name, Profit/Loss, Margin %, Job Count
- [ ] Color coding: green for profitable customers, red for unprofitable
- [ ] Answers the question: "Which customers are actually profitable?"
- [ ] Data is tenant-scoped

---

## Page Layout

```
┌─ Customer Profitability ───────────────────────────────────────┐
│                                                                 │
│  Customer          │ Jobs │ Revenue   │ Cost      │ P/L    │ % │
│  ──────────────────│──────│───────────│───────────│────────│───│
│  ABC Fabrication   │  12  │ $45,200   │ $38,100   │ +$7.1k │22%│
│  XYZ Manufacturing │   8  │ $31,000   │ $28,500   │ +$2.5k │ 8%│
│  Smith & Sons      │   5  │ $18,400   │ $19,200   │ -$800  │-4%│
│  Metro Industries  │   3  │ $12,800   │ $10,100   │ +$2.7k │21%│
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Data source: group completed jobs by `CustomerName`, aggregate costs and revenue
- Service method: `IDashboardService.GetCustomerProfitabilityAsync()`
- DTO:
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
- Can be displayed as a table (sortable) or a horizontal bar chart
- Consider Chart.js horizontal bar chart as an alternative/complement to the table
- Sorting: client-side with JavaScript or server-side with query parameters
- Handle customers with no completed jobs (exclude from the list)

---

## Dependencies

- Feature 7.1 (Main Dashboard)
- Feature 3.2 (Job Entity — customer name)
- Feature 5.1 (Job Actuals — actual revenue and costs)
- Feature 6.1 (Profitability Service)

---

## Definition of Done

- [ ] Customer profitability data displayed on dashboard
- [ ] Aggregation is correct across multiple jobs per customer
- [ ] Sorting works on all columns
- [ ] Color coding indicates profitable vs unprofitable customers
- [ ] Data is tenant-scoped
- [ ] Manual smoke test with multiple customers and jobs
