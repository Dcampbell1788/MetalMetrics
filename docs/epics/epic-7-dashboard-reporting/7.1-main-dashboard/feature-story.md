# Feature 7.1 — Main Dashboard

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** company owner or project manager,
**I want** to see key business metrics at a glance when I log in,
**so that** I can quickly assess the health of my fabrication business without digging through individual jobs.

---

## Acceptance Criteria

- [ ] Dashboard is the landing page after login (`/Dashboard`)
- [ ] Displays KPI cards in a top row:
  - Total Jobs (this month / all time)
  - Average Margin %
  - Jobs Over Budget (count of jobs with actual margin below target)
  - Revenue This Month
- [ ] KPI cards are visually distinct with icons or colored accents
- [ ] Dashboard data is scoped to the current tenant
- [ ] Data only includes completed jobs with actuals for margin/budget metrics
- [ ] Empty state handled gracefully ("No completed jobs yet")
- [ ] Access: Admin, Owner, ProjectManager, Foreman

---

## Page Layout

```
┌─────────────────────────────────────────────────────────────────┐
│  Dashboard                                          Welcome, John │
├────────────────┬────────────────┬───────────────┬───────────────┤
│  Total Jobs    │  Avg Margin    │ Over Budget   │ Revenue (Mo)  │
│                │                │               │               │
│     42         │    18.3%       │     7         │   $52,400     │
│  8 this month  │  Target: 20%  │  ⚠️ 16.7%    │               │
└────────────────┴────────────────┴───────────────┴───────────────┘
│                                                                   │
│  (Charts from Features 7.2–7.5 render below)                     │
│                                                                   │
└───────────────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Page: `Pages/Dashboard/Index.cshtml` + `Index.cshtml.cs`
- Create a `IDashboardService` / `DashboardService` to aggregate KPI data
- KPI queries:
  - Total Jobs: `COUNT(*)` from `Jobs` where `TenantId = current`
  - This Month: filter by `CreatedAt` in current month
  - Avg Margin: `AVG(ActualMarginPercent)` across completed jobs with actuals
  - Over Budget: `COUNT(*)` where `ActualMarginPercent < TargetMarginPercent`
  - Revenue This Month: `SUM(ActualRevenue)` where completed this month
- Use Bootstrap cards for KPI display
- Consider caching dashboard data if queries are slow (unlikely with SQLite for demo)
- Set as the default authenticated page in routing

---

## Dependencies

- Feature 1.4 (Shared Layout)
- Feature 3.2 (Job Entity — for job counts)
- Feature 5.1 (Job Actuals — for revenue and margin data)
- Feature 6.1 (Profitability Service — for margin calculations)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] Dashboard renders as the post-login landing page
- [ ] All 4 KPI cards display correct data
- [ ] Data is tenant-scoped
- [ ] Empty state handled for new tenants
- [ ] Role-based access enforced
- [ ] Manual smoke test passed
