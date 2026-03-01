# Feature 8.1 — Seed Data Generator

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Complete

---

## User Story

**As a** demo presenter,
**I want** the app pre-loaded with realistic sheetmetal jobs, estimates, and actuals,
**so that** the dashboard and reports tell a compelling story.

---

## Implementation

### DbSeeder (`Infrastructure/Data/DbSeeder.cs`, ~460 lines)

Static class called from `Program.cs` in Development environment only:
```csharp
if (app.Environment.IsDevelopment())
{
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}
```

### Idempotency

Checks `context.Tenants.Any()` before seeding. Safe to run multiple times.

### Fixed Random Seed

Uses `new Random(42)` for reproducible demo data across rebuilds.

### Two Demo Tenants

**Precision Metal Works** (profitable shop):
- 10 employees, 24 jobs
- Rates: $95/hr labor, $175/hr machine, 30% overhead, 25% target margin
- Customers: Cascade Structural, Pacific Rim Manufacturing, Olympic Steel Systems, Summit Engineering, Columbia Basin Metals, Rainier Industrial
- Variance profile: tight control, +/-5-10% estimate variance

**Budget Fabricators** (struggling shop):
- 7 employees, 16 jobs
- Rates: $68/hr labor, $130/hr machine, 20% overhead, 15% target margin
- Customers: ABC Fabrication, Metro Industries, Smith & Sons, Valley Contractors, Delta Manufacturing
- Variance profile: 20-30%+ variance, every 3rd job has major overrun

### Job Profiles (24 types)

Realistic sheetmetal work: laser cut brackets, welded enclosures, CNC punched plates, formed channels, stainless handrails, aluminum panels, etc. Each with appropriate labor hours (2-40), material costs ($50-$5,000), machine hours (0.5-15), and quote prices ($500-$15,000).

### Seed Data Includes

1. **Roles** — All 6 AppRole values created as IdentityRoles
2. **Tenants** + TenantSettings with appropriate rates
3. **Users** — All demo users with hashed passwords (`Demo123!`)
4. **Jobs** — Mix of Quoted (2), InProgress (2), Completed (most), Invoiced
5. **JobEstimates** — Cost breakdowns for each job
6. **JobActuals** — For completed/invoiced jobs with variance modeling
7. **JobAssignments** — PMs, Estimators, Foremen, Journeymen assigned to jobs
8. **JobTimeEntries** — 2-4 entries per worker per job with realistic hours
9. **JobNotes** — Sample progress notes with placeholder SVG image references

### Completion Date Spread

Jobs completed over the last 3-6 months for realistic chart data.

---

## Definition of Done

- [x] 2 tenants with 40 total jobs and 17 users
- [x] Mix of profitable and unprofitable outcomes
- [x] All estimates and actuals have realistic values
- [x] Seed is idempotent
- [x] Fixed random seed for reproducibility
- [x] Demo credentials documented (all `Demo123!`)
- [x] Dashboard and charts look compelling with seed data
