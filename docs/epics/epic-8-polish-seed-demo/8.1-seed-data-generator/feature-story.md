# Feature 8.1 — Seed Data Generator

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** demo presenter,
**I want** the application pre-loaded with realistic sheetmetal fabrication jobs, estimates, and actuals,
**so that** the dashboard and reports look compelling and the demo tells a convincing story.

---

## Acceptance Criteria

- [ ] Seed data generator creates 15–25 realistic sheetmetal jobs
- [ ] Jobs include a mix of profitable and unprofitable outcomes
- [ ] Varied customer names (at least 5–6 different customers)
- [ ] Varied materials (mild steel, stainless, aluminum, etc.)
- [ ] Varied complexity levels and operations
- [ ] Each job has a pre-populated `JobEstimate`
- [ ] Each completed job has pre-populated `JobActuals`
- [ ] At least 2 demo tenants (companies) with separate data
- [ ] Job statuses are mixed: some Quoted, some InProgress, most Completed, a few Invoiced
- [ ] Seed data runs on application startup (or via a command/flag)
- [ ] Seed data is idempotent (doesn't duplicate on re-run)

---

## Seed Data Profile

| Attribute        | Range / Distribution                                      |
|------------------|-----------------------------------------------------------|
| Customers        | 5–6 companies (ABC Fabrication, Metro Industries, etc.)   |
| Materials        | Mild Steel (40%), Stainless (25%), Aluminum (20%), Other  |
| Labor Hours      | 2–40 hours per job                                        |
| Material Cost    | $50–$5,000 per job                                        |
| Machine Hours    | 0.5–15 hours per job                                      |
| Quote Prices     | $500–$15,000                                              |
| Margin Outcomes  | 60% profitable, 25% marginal, 15% at a loss              |
| Completion Dates | Spread over the last 3–6 months                           |

---

## Technical Notes

- Location: `MetalMetrics.Infrastructure/Data/DbSeeder.cs` or `SeedDataGenerator.cs`
- Call from `Program.cs`:
  ```csharp
  using (var scope = app.Services.CreateScope())
  {
      var seeder = scope.ServiceProvider.GetRequiredService<IDbSeeder>();
      await seeder.SeedAsync();
  }
  ```
- Idempotency: check if data already exists before seeding (`if (!context.Jobs.Any())`)
- Create 2 tenants:
  - "Precision Metal Works" — well-run shop (mostly profitable)
  - "Budget Fabricators" — struggling shop (more losses, for contrast)
- Include at least one dramatic "money lost" job for demo impact
- Use realistic sheetmetal pricing ranges (research if needed)
- Consider creating demo user accounts with known passwords for easy login

---

## Dependencies

- Feature 3.1 (Tenant Entity)
- Feature 3.2 (Job Entity)
- Feature 3.3 (Quote Entity)
- Feature 3.4 (Tenant Settings)
- Feature 5.1 (Job Actuals Entity)
- Feature 2.1 (Identity — for demo user accounts)

---

## Definition of Done

- [ ] Seed data populates 15–25 jobs across 2 tenants
- [ ] Mix of profitable and unprofitable jobs
- [ ] All estimates and actuals have realistic values
- [ ] Dashboard and charts look compelling with seed data
- [ ] Seed is idempotent (safe to run multiple times)
- [ ] Demo user accounts created with known credentials
- [ ] Manual verification that reports look realistic
