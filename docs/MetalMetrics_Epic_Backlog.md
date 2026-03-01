# MetalMetrics — Epic & Feature Backlog

**Project:** MetalMetrics — Sheetmetal Fabrication Job Profitability Tracker
**Tech Stack:** .NET 8 Razor Pages, C#, SQLite, EF Core 8, ASP.NET Identity, Claude AI API, Chart.js v4, Bootstrap 5
**Status:** All 8 Epics Complete (MVP v1.0)

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     RAZOR PAGES UI                          │
│  Dashboard · Jobs · Quotes · Actuals · Admin · Reports      │
│  Notes · TimeEntries · Assignments · Profitability          │
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION SERVICES                      │
│  JobService · QuoteService · ActualsService · AIQuoteService│
│  ProfitabilityService · DashboardService · ReportsService   │
│  JobAssignmentService · TimeEntryService · JobNoteService   │
├─────────────────────────────────────────────────────────────┤
│                     DATA / EF CORE                          │
│  SQLite · Multi-Tenant AppDbContext · 10 Entities           │
│  Global Query Filter · Auto-Audit · Auto-TenantId          │
├─────────────────────────────────────────────────────────────┤
│                     EXTERNAL SERVICES                       │
│  Claude AI API (IHttpClientFactory, retry/backoff)          │
│  Chart.js v4 + chartjs-plugin-annotation (CDN)             │
└─────────────────────────────────────────────────────────────┘
```

### Solution Structure

```
MetalMetrics/
├── MetalMetrics.Core/              Entities, DTOs, Enums, Interfaces
├── MetalMetrics.Infrastructure/    EF Core (SQLite), Services, Migrations, Seeder
├── MetalMetrics.Web/               Razor Pages, Program.cs, wwwroot
├── MetalMetrics.Tests/             MSTest unit tests (20 tests)
└── docs/                           Epic & feature documentation
```

### Multi-Tenant Strategy

- **Row-level tenancy** via `TenantId` (Guid) on every `BaseEntity`
- `AppDbContext.SaveChangesAsync()` auto-sets `TenantId` from `ITenantProvider` for new entities
- `ITenantProvider` resolves tenant from the authenticated user's `TenantId` claim
- No global query filter — tenant scoping done in service layer queries with `.Where(j => j.TenantId == tenantId)`
- Unique indexes on `(TenantId, JobNumber)` and `(TenantId, Slug)` for Job entity

### Role Hierarchy & Authorization Policies

| Role           | Dashboard | Jobs | Quotes | Actuals | Reports | Admin |
|----------------|-----------|------|--------|---------|---------|-------|
| Admin          | Yes       | Full | Full   | Full    | Yes     | Yes   |
| Owner          | Yes       | Full | Full   | Full    | Yes     | Yes   |
| ProjectManager | Yes       | Full | Full   | Full    | Yes     | No    |
| Foreman        | No        | Assigned | No | Full    | No      | No    |
| Estimator      | No        | Assigned | Full | No    | No      | No    |
| Journeyman     | No        | Assigned | No | Full    | No      | No    |

**Authorization Policies (Program.cs):**
- `AdminOnly` -> Admin, Owner
- `CanManageJobs` -> Admin, Owner, ProjectManager, Foreman
- `CanQuote` -> Admin, Owner, ProjectManager, Estimator
- `CanEnterActuals` -> Admin, Owner, ProjectManager, Foreman
- `CanViewReports` -> Admin, Owner, ProjectManager
- `CanAssignJobs` -> Admin, Owner, ProjectManager, Foreman

### Entity Relationship Diagram

```
Tenant (1) ──── (*) AppUser
  │
  ├──── (1) TenantSettings
  │
  └──── (*) Job
              ├──── (0..1) JobEstimate
              ├──── (0..1) JobActuals
              ├──── (*) JobAssignment ──── AppUser
              ├──── (*) JobTimeEntry ──── AppUser
              └──── (*) JobNote
```

### Key Entities Summary

| Entity           | Key Fields                                                             |
|------------------|------------------------------------------------------------------------|
| `BaseEntity`     | Id (Guid), TenantId (Guid), CreatedAt, UpdatedAt (IAuditable)          |
| `Tenant`         | CompanyName                                                            |
| `AppUser`        | FullName, TenantId, Role (AppRole), extends IdentityUser               |
| `TenantSettings` | DefaultLaborRate, DefaultMachineRate, DefaultOverheadPercent, TargetMarginPercent |
| `Job`            | JobNumber, Slug, CustomerName, Description, Status, CompletedAt        |
| `JobEstimate`    | LaborHrs, LaborRate, MaterialCost, MachineHrs, MachineRate, OverheadPct, TotalEstimatedCost, QuotePrice, EstimatedMarginPercent, AIGenerated, AIPromptSnapshot, CreatedBy |
| `JobActuals`     | ActualLaborHrs, LaborRate, MaterialCost, ActualMachineHrs, MachineRate, OverheadPct, TotalActualCost, ActualRevenue, Notes, EnteredBy |
| `JobAssignment`  | JobId, UserId, AssignedByUserId                                        |
| `JobTimeEntry`   | JobId, UserId, HoursWorked, WorkDate, Notes                           |
| `JobNote`        | JobId, UserId, AuthorName, Content, ImageFileName                      |

### Key DTOs

| DTO                       | Purpose                                    |
|---------------------------|--------------------------------------------|
| `AIQuoteRequest`          | Form input to Claude API                   |
| `AIQuoteResponse`         | Parsed Claude JSON response                |
| `JobSummaryDto`           | Dashboard chart data per completed job     |
| `DashboardKpiDto`         | Top-level KPI metrics                      |
| `JobProfitabilityReport`  | Detailed per-job P&L with variances        |
| `VarianceDetail`          | Cost category variance (est/actual/$/%     |
| `CustomerProfitabilityDto`| Revenue/cost/P&L/margin per customer       |
| `CategoryVarianceDto`     | Avg variance % + $ per cost category       |
| `AtRiskJobDto`            | InProgress jobs over budget threshold      |

### Demo Credentials

All passwords: `Demo123!`

**Precision Metal Works** (profitable, 24 jobs, 10 employees):

| Role           | Email                      |
|----------------|----------------------------|
| Owner          | mike@precisionmetal.demo   |
| Admin          | karen@precisionmetal.demo  |
| ProjectManager | dave@precisionmetal.demo   |
| ProjectManager | lisa@precisionmetal.demo   |
| Estimator      | rich@precisionmetal.demo   |
| Foreman        | tony@precisionmetal.demo   |
| Foreman        | brenda@precisionmetal.demo |
| Journeyman     | jake@precisionmetal.demo   |
| Journeyman     | sam@precisionmetal.demo    |
| Journeyman     | cody@precisionmetal.demo   |

**Budget Fabricators** (struggling, 16 jobs, 7 employees):

| Role           | Email                  |
|----------------|------------------------|
| Owner          | daryl@budgetfab.demo   |
| Admin          | janet@budgetfab.demo   |
| ProjectManager | greg@budgetfab.demo    |
| Estimator      | megan@budgetfab.demo   |
| Foreman        | ray@budgetfab.demo     |
| Journeyman     | tyler@budgetfab.demo   |
| Journeyman     | nick@budgetfab.demo    |

---

## EPIC 1 — Project Scaffold & Infrastructure [COMPLETE]

**Goal:** Bootable .NET 8 Razor Pages app with SQLite, EF Core, multi-tenant base, and shared layout.

| Feature | Description | Status |
|---------|-------------|--------|
| 1.1 | Solution & Project Structure | Complete |
| 1.2 | EF Core + SQLite Setup | Complete |
| 1.3 | Base Entity Model | Complete |
| 1.4 | Shared Layout & Navigation | Complete |

---

## EPIC 2 — Authentication, Tenancy & Role Management [COMPLETE]

**Goal:** Users can register a company (tenant), log in, and invite team members with roles.

| Feature | Description | Status |
|---------|-------------|--------|
| 2.1 | ASP.NET Identity Setup | Complete |
| 2.2 | Tenant Registration (Company Onboarding) | Complete |
| 2.3 | Login / Logout | Complete |
| 2.4 | User Invitation & Role Assignment | Complete |
| 2.5 | Role-Based Authorization | Complete |

---

## EPIC 3 — Job & Quote Management [COMPLETE]

**Goal:** Create and manage jobs with structured cost estimates. Auto-generated job numbers, slug-based URLs, full CRUD.

| Feature | Description | Status |
|---------|-------------|--------|
| 3.1 | Tenant Entity | Complete |
| 3.2 | Job Entity & CRUD | Complete |
| 3.3 | Quote / Estimate Entity | Complete |
| 3.4 | Cost Category Configuration (Tenant Settings) | Complete |

---

## EPIC 4 — AI-Powered Quoting [COMPLETE]

**Goal:** Estimator fills structured form, Claude AI suggests pricing based on sheetmetal domain knowledge.

| Feature | Description | Status |
|---------|-------------|--------|
| 4.1 | Claude API Client Service | Complete |
| 4.2 | Structured Quote Request Form | Complete |
| 4.3 | AI Prompt Engineering | Complete |
| 4.4 | AI Quote Review & Acceptance | Complete |

---

## EPIC 5 — Actuals Entry [COMPLETE]

**Goal:** After a job completes, record actual costs to compare against the estimate.

| Feature | Description | Status |
|---------|-------------|--------|
| 5.1 | Job Actuals Entity | Complete |
| 5.2 | Actuals Entry Form (side-by-side) | Complete |
| 5.3 | Quick Entry Mode (Journeyman) | Complete |
| 5.4 | Job Completion Workflow | Complete |

---

## EPIC 6 — Profitability Engine [COMPLETE]

**Goal:** One-click view showing whether a job made or lost money, broken down by category.

| Feature | Description | Status |
|---------|-------------|--------|
| 6.1 | Profitability Calculation Service | Complete |
| 6.2 | Per-Job Profitability View | Complete |
| 6.3 | Margin Threshold Alerts | Complete |

---

## EPIC 7 — Dashboard & Reporting [COMPLETE]

**Goal:** At-a-glance overview of business health with charts, KPIs, pipeline visibility, and at-risk alerts.

| Feature | Description | Status |
|---------|-------------|--------|
| 7.1 | Main Dashboard (KPIs + Pipeline) | Complete |
| 7.2 | Profitability Summary Chart (Est vs Actual, color-coded) | Complete |
| 7.3 | Margin Drift Trend (with target annotation line) | Complete |
| 7.4 | Customer Profitability Breakdown (sortable, row highlighting) | Complete |
| 7.5 | Category Variance / Estimating Accuracy (% + $ + arrows) | Complete |
| 7.6 | At-Risk Job Alerts | Complete |
| 7.7 | Work Pipeline Section | Complete |
| 7.8 | Reports Page (date-range filtering) | Complete |

---

## EPIC 8 — Polish, Seed Data & Demo Prep [COMPLETE]

**Goal:** Demo-ready with realistic data, professional UX, error handling, and print styles.

| Feature | Description | Status |
|---------|-------------|--------|
| 8.1 | Seed Data Generator (2 tenants, 40 jobs, 17 users) | Complete |
| 8.2 | UX Polish (responsive, color palette, print CSS) | Complete |
| 8.3 | Error Handling & Edge Cases | Complete |
| 8.4 | Demo Script Preparation | Complete |

---

## Service Registration (Program.cs)

All services are registered as scoped in `Program.cs`:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IQuoteService, QuoteService>();
builder.Services.AddScoped<IActualsService, ActualsService>();
builder.Services.AddScoped<IProfitabilityService, ProfitabilityService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportsService, ReportsService>();
builder.Services.AddScoped<IJobAssignmentService, JobAssignmentService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<IJobNoteService, JobNoteService>();
builder.Services.AddHttpClient<IAIQuoteService, ClaudeAIQuoteService>();
```

## Migrations

7 migrations applied automatically on startup:

1. `InitialIdentityAndTenant` - Identity schema, Tenant, AppUser
2. `AddTenantSettings` - TenantSettings with defaults
3. `AddJobAndJobEstimate` - Job, JobEstimate entities
4. `AddJobActuals` - JobActuals entity
5. `AddJobAssignmentTimeEntryNotes` - JobAssignment, JobTimeEntry, JobNote
6. `AddTimeEntryUserNavigation` - FK: TimeEntry.User
7. `AddJobSlug` - Job.Slug (8-char unique per tenant)

## Testing

20 MSTest unit tests across 6 test classes using EF Core InMemory provider:

| Test Class | Tests | Coverage |
|-----------|-------|----------|
| BaseEntityTests | 3 | Guid generation, default TenantId |
| AppDbContextTests | 3 | Audit timestamps, auto TenantId |
| JobServiceTests | 5 | JobNumber generation, status transitions, search/filter |
| QuoteServiceTests | 2 | Cost/margin calculation, zero edge case |
| ActualsServiceTests | 2 | Cost calculation, zero edge case |
| ProfitabilityServiceTests | 5 | Profit/loss/break-even scenarios, warnings |

## Key Design Decisions

1. **SQLite** for zero-config development and demo portability
2. **No global query filter** — tenant scoping done explicitly in service queries for clarity
3. **Fixed random seed (42)** in DbSeeder for reproducible demo data
4. **IHttpClientFactory** for Claude API with retry/exponential backoff
5. **TempData** for passing AI quote data between pages (multi-step workflow)
6. **Chart.js v4 via CDN** — no npm build required
7. **Slug-based URLs** — 8-char random slug for shareable job links
8. **Session-based filters** — PRG pattern for job list search/status persistence
9. **Cascade deletes** — Tenant->Settings, Job->Estimate/Actuals/Assignments/TimeEntries/Notes
10. **SQLite decimal limitation** — aggregate `Sum` on decimals done client-side via `ToListAsync()` then LINQ

## Configuration

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=metalmetrics.db"
  }
}
```

**User Secrets (not in repo):**
- `ClaudeAI:ApiKey` — Anthropic API key
- `ClaudeAI:Model` — Model name (default: `claude-sonnet-4-6`)
- `ClaudeAI:MaxTokens` — Response limit (default: 1024)
- `ClaudeAI:TimeoutSeconds` — Request timeout (default: 30)
