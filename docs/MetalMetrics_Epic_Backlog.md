# MetalMetrics — Epic & Feature Backlog

**Team:** MetalMetrics
**Event:** St. Joseph Vibeathon — "Did We Make Money?"
**Sprint Window:** 24 Hours
**Tech Stack:** .NET 8 Razor Pages · C# · SQLite · EF Core · Claude AI API

---

## Architecture Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     RAZOR PAGES UI                          │
│  Dashboard · Jobs · Quotes · Actuals · Admin · Reports      │
├─────────────────────────────────────────────────────────────┤
│                    APPLICATION SERVICES                      │
│  QuotingService · JobService · ActualsService · AIService    │
│  ProfitabilityEngine · TenantService · AuthService           │
├─────────────────────────────────────────────────────────────┤
│                     DATA / EF CORE                           │
│  SQLite · Multi-Tenant DbContext · Entities · Configs        │
├─────────────────────────────────────────────────────────────┤
│                     EXTERNAL SERVICES                        │
│  Claude AI API (Structured Quoting)                          │
└─────────────────────────────────────────────────────────────┘
```

### Multi-Tenant Strategy

- **Row-level tenancy** via `TenantId` on every entity
- Global query filter in `DbContext` automatically scopes all queries
- `ITenantProvider` resolves tenant from the authenticated user's claims
- Simplest approach for SQLite + hackathon timeline

### Role Hierarchy

| Role              | Jobs | Quotes | Actuals | Reports | Admin/Billing |
|-------------------|------|--------|---------|---------|---------------|
| Admin             | ✅   | ✅     | ✅      | ✅      | ✅            |
| Owner             | ✅   | ✅     | ✅      | ✅      | ✅            |
| ProjectManager    | ✅   | ✅     | ✅      | ✅      | ❌            |
| Foreman           | ✅   | ✅     | ✅      | ✅      | ❌            |
| Estimator         | 👁️   | ✅     | ❌      | ❌      | ❌            |
| Journeyman        | 👁️   | ❌     | ✅      | ❌      | ❌            |

---

## Sprint Timeline (Suggested 24-Hour Breakdown)

| Phase          | Hours | Epics                                      |
|----------------|-------|--------------------------------------------|
| **Foundation** | 0–4   | Epic 1 (Scaffold) + Epic 2 (Auth/Tenant)   |
| **Core Data**  | 4–8   | Epic 3 (Jobs + Quoting Data Model)         |
| **AI + Entry** | 8–14  | Epic 4 (AI Quoting) + Epic 5 (Actuals)     |
| **Insights**   | 14–20 | Epic 6 (Profitability) + Epic 7 (Dashboard) |
| **Polish**     | 20–24 | Epic 8 (Polish, Demo Prep, Bug Fixes)       |

### Cut Lines (If Running Behind)

- **Must Ship:** Epics 1–6 (core loop: quote → actuals → profit/loss)
- **High Value:** Epic 7 (dashboard charts — huge demo impact)
- **Nice to Have:** Epic 8 polish items, advanced AI features

---

## EPIC 1 — Project Scaffold & Infrastructure

**Goal:** Bootable .NET 8 Razor Pages app with SQLite, EF Core, and project structure.

### Features

#### 1.1 Solution & Project Structure
- Create solution `MetalMetrics.sln`
- Projects: `MetalMetrics.Web` (Razor Pages), `MetalMetrics.Core` (domain/services/interfaces), `MetalMetrics.Infrastructure` (EF Core, AI client), `MetalMetrics.Tests` (MSTest)
- Configure `.editorconfig`, `global.json`

#### 1.2 EF Core + SQLite Setup
- Install EF Core SQLite packages
- Create `AppDbContext` with multi-tenant global query filter
- Create `ITenantProvider` interface + implementation
- Initial migration + database seeding script
- Connection string in `appsettings.json`

#### 1.3 Base Entity Model
- `BaseEntity` abstract class: `Id (Guid)`, `TenantId (Guid)`, `CreatedAt`, `UpdatedAt`
- `IAuditable` interface for automatic timestamp setting
- `SaveChangesAsync` override for audit fields

#### 1.4 Shared Layout & Navigation
- `_Layout.cshtml` with responsive nav (role-aware menu items)
- Branding: "MetalMetrics" logo/text
- Toast/notification partial for success/error messages
- CSS framework decision (Bootstrap 5 — ships with .NET template)

---

## EPIC 2 — Authentication, Tenancy & Role Management

**Goal:** Users can register a company (tenant), log in, and invite team members with roles.

### Features

#### 2.1 ASP.NET Identity Setup
- Extend `IdentityUser` → `AppUser` with `TenantId`, `Role`, `FullName`
- Configure Identity with SQLite store
- Cookie authentication scheme

#### 2.2 Tenant Registration (Company Onboarding)
- **Page:** `/Register`
- Form: Company Name, Owner Name, Email, Password
- Creates `Tenant` entity + first `AppUser` as Admin
- Auto-login after registration

#### 2.3 Login / Logout
- **Page:** `/Login`, `/Logout`
- Standard Identity login with email/password
- Redirect to Dashboard on success

#### 2.4 User Invitation & Role Assignment
- **Page:** `/Admin/Users` (Admin/Owner only)
- Invite user by email with assigned role
- Roles: `Admin`, `Owner`, `ProjectManager`, `Foreman`, `Estimator`, `Journeyman`
- Simple invite flow (pre-create user record, set temp password or invite link)

#### 2.5 Role-Based Authorization
- Custom `AuthorizationHandler` or policy-based auth
- Page-level `[Authorize(Roles = "...")]` attributes
- Navigation menu hides links based on role
- `TenantProvider` reads `TenantId` from user claims

---

## EPIC 3 — Job & Quote Management (Core Data)

**Goal:** Create and manage jobs with structured cost estimates across all four categories.

### Features

#### 3.1 Tenant Entity
- `Tenant`: `Id`, `CompanyName`, `CreatedAt`
- One-to-many with `AppUser`, `Job`

#### 3.2 Job Entity & CRUD
- **Entity:** `Job`
  - `Id`, `TenantId`, `JobNumber` (auto-gen), `CustomerName`, `Description`
  - `Status` (enum: `Quoted`, `InProgress`, `Completed`, `Invoiced`)
  - `CreatedAt`, `UpdatedAt`, `CompletedAt`
- **Pages:** `/Jobs/Index`, `/Jobs/Create`, `/Jobs/Details/{id}`, `/Jobs/Edit/{id}`
- List view with search, filter by status
- Access: Admin, Owner, ProjectManager, Foreman, Estimator (read-only), Journeyman (read-only)

#### 3.3 Quote / Estimate Entity
- **Entity:** `JobEstimate`
  - `Id`, `JobId`, `TenantId`
  - `EstimatedLaborHours`, `LaborRate`
  - `EstimatedMaterialCost` (sheet/plate cost)
  - `EstimatedMachineHours`, `MachineRate`
  - `OverheadPercent` (applied to subtotal)
  - `TotalEstimatedCost` (computed)
  - `QuotePrice` (what they charge the customer)
  - `EstimatedMarginPercent` (computed)
  - `AIGenerated` (bool), `AIPromptSnapshot` (JSON — what was sent to Claude)
  - `CreatedBy`, `CreatedAt`
- **Pages:** `/Jobs/{jobId}/Quote/Create`, `/Jobs/{jobId}/Quote/View`
- Access: Admin, Owner, ProjectManager, Estimator

#### 3.4 Cost Category Configuration (Tenant-Level Rates)
- **Entity:** `TenantSettings`
  - `DefaultLaborRate`, `DefaultMachineRate`, `DefaultOverheadPercent`
- **Page:** `/Admin/Settings` (Admin/Owner only)
- Pre-populates quote forms with tenant defaults

---

## EPIC 4 — AI-Powered Quoting (Claude Integration)

**Goal:** Estimator fills structured form, Claude AI suggests pricing based on sheetmetal domain knowledge.

### Features

#### 4.1 Claude API Client Service
- `IAIQuoteService` interface
- `ClaudeAIQuoteService` implementation
- Reads API key from `appsettings.json` (or user secrets)
- Structured prompt template for sheetmetal quoting
- Rate limiting / error handling

#### 4.2 Structured Quote Request Form
- **Page:** `/Jobs/{jobId}/Quote/AI`
- Form fields:
  - Material type (dropdown: Mild Steel, Stainless, Aluminum, Galvanized, etc.)
  - Material thickness (gauge or decimal inches)
  - Part dimensions (L × W) or sheet size needed
  - Quantity
  - Operations required (checkboxes: Laser Cut, Brake/Bend, Punch, Weld, Deburr)
  - Complexity level (Simple, Moderate, Complex)
  - Any special notes (free text)
- Submit → calls Claude API

#### 4.3 AI Prompt Engineering
- System prompt: "You are a sheetmetal fabrication estimating expert..."
- Include tenant's default rates in the prompt context
- Request structured JSON response:
  ```json
  {
    "estimatedLaborHours": 4.5,
    "estimatedMaterialCost": 285.00,
    "estimatedMachineHours": 2.0,
    "overheadPercent": 15,
    "suggestedQuotePrice": 1250.00,
    "reasoning": "...",
    "assumptions": ["..."],
    "confidenceLevel": "Medium"
  }
  ```
- Parse response and pre-fill `JobEstimate` form

#### 4.4 AI Quote Review & Acceptance
- Display Claude's suggestions alongside the form
- Show reasoning and assumptions
- Estimator can adjust any values before saving
- Track whether final quote used AI suggestions (`AIGenerated` flag)
- Save prompt + response snapshot for auditing

---

## EPIC 5 — Actuals Entry (Post-Job Tracking)

**Goal:** After a job completes, record actual costs to compare against the estimate.

### Features

#### 5.1 Job Actuals Entity
- **Entity:** `JobActuals`
  - `Id`, `JobId`, `TenantId`
  - `ActualLaborHours`, `LaborRate` (may differ from estimate)
  - `ActualMaterialCost`
  - `ActualMachineHours`, `MachineRate`
  - `OverheadPercent`
  - `TotalActualCost` (computed)
  - `ActualRevenue` (what they actually invoiced/collected)
  - `Notes`
  - `EnteredBy`, `CreatedAt`, `UpdatedAt`

#### 5.2 Actuals Entry Form
- **Page:** `/Jobs/{jobId}/Actuals/Enter`
- Pre-populate with estimated values as starting reference
- Side-by-side: Estimate column | Actuals column
- Real-time variance calculation as user types
- Color coding: green (under estimate), red (over estimate)
- Access: Admin, Owner, ProjectManager, Foreman, Journeyman

#### 5.3 Quick Entry Mode (Journeyman Optimized)
- **Page:** `/Jobs/{jobId}/Actuals/Quick`
- Simplified mobile-friendly form
- Only: Labor hours, Material cost, Machine hours, Notes
- Big buttons, minimal fields
- Auto-fills rates from tenant settings

#### 5.4 Job Completion Workflow
- "Mark as Completed" action on Job
- Prompts to enter actuals if not yet entered
- Updates `Job.Status` → `Completed`, sets `CompletedAt`
- Validates that actuals exist before allowing status change to `Invoiced`

---

## EPIC 6 — Profitability Engine & Per-Job Analysis

**Goal:** One-click view showing whether a job made or lost money, broken down by category.

### Features

#### 6.1 Profitability Calculation Service
- `IProfitabilityService`
- Inputs: `JobEstimate` + `JobActuals`
- Outputs: `JobProfitabilityReport`
  ```
  {
    LaborVariance ($, %),
    MaterialVariance ($, %),
    MachineVariance ($, %),
    OverheadVariance ($, %),
    TotalEstimatedCost,
    TotalActualCost,
    QuotedPrice,
    ActualRevenue,
    EstimatedMargin ($, %),
    ActualMargin ($, %),
    MarginDrift ($, %),
    OverallVerdict: "Profit" | "Loss" | "Break Even",
    Warnings: []
  }
  ```

#### 6.2 Per-Job Profitability View
- **Page:** `/Jobs/{jobId}/Profitability`
- Hero metric: ✅ PROFIT +$X,XXX or ❌ LOSS -$X,XXX
- Stacked bar chart: Estimated vs Actual by category (Labor, Material, Machine, Overhead)
- Red/green color coding for each category variance
- Margin drift indicator
- Warning badges (e.g., "Material cost exceeded estimate by 35%")
- Access: Admin, Owner, ProjectManager

#### 6.3 Margin Threshold Alerts
- Configurable in `TenantSettings`: `TargetMarginPercent` (default: 20%)
- Flag jobs where actual margin drops below threshold
- Visual warning on job list and profitability view

---

## EPIC 7 — Dashboard & Reporting

**Goal:** At-a-glance overview of business health across all jobs.

### Features

#### 7.1 Main Dashboard
- **Page:** `/Dashboard` (landing page after login)
- KPI Cards (top row):
  - Total Jobs (this month / all time)
  - Average Margin %
  - Jobs Over Budget (count)
  - Revenue This Month
- Access: Admin, Owner, ProjectManager, Foreman

#### 7.2 Profitability Summary Chart
- Bar chart: All completed jobs — Estimated vs Actual Cost
- Color: Green bars (profitable), Red bars (loss)
- Sortable by: Date, Margin %, Customer
- Chart library: Chart.js via CDN (lightweight, no npm needed)

#### 7.3 Margin Drift Trend
- Line chart: Margin % over time (by job completion date)
- Shows target margin threshold line
- Helps spot if quoting accuracy is improving or degrading

#### 7.4 Customer Profitability Breakdown
- Table or chart: Profit/Loss by customer
- "Which customers are actually profitable?"
- Aggregates all jobs per customer

#### 7.5 Category Variance Heatmap
- Which cost category do you consistently underestimate?
- Aggregated view: Average variance by Labor, Material, Machine, Overhead
- Helps improve future quoting accuracy

---

## EPIC 8 — Polish, Seed Data & Demo Prep

**Goal:** Make it demo-ready with realistic data and professional UX.

### Features

#### 8.1 Seed Data Generator
- Create 15–25 realistic sheetmetal jobs
- Mix of profitable and unprofitable
- Varied customers, materials, complexities
- Pre-populated estimates and actuals
- At least 2 demo tenants (companies)

#### 8.2 UX Polish
- Loading spinners on AI quote requests
- Form validation messages (server + client)
- Responsive design check (tablet/mobile for Journeyman view)
- Consistent color palette: Green = profit, Red = loss, Blue = neutral
- Print-friendly profitability report view

#### 8.3 Error Handling & Edge Cases
- Claude API timeout / failure graceful degradation
- Empty states (no jobs yet, no actuals entered)
- Prevent duplicate actuals entry
- Handle division-by-zero in margin calculations

#### 8.4 Demo Script Preparation
- Happy path walkthrough:
  1. Register company → set up rates
  2. Create job → AI quote → accept quote
  3. Enter actuals → view profitability → "Did we make money?"
  4. Dashboard overview → customer insights
- Have one "money lost" job highlighted for dramatic effect

---

## Entity Relationship Diagram (Simplified)

```
Tenant (1) ──── (*) AppUser
  │
  ├──── (*) Job
  │         ├──── (1) JobEstimate
  │         └──── (1) JobActuals
  │
  └──── (1) TenantSettings
```

### Key Entities Summary

| Entity          | Key Fields                                                        |
|-----------------|-------------------------------------------------------------------|
| `Tenant`        | CompanyName                                                       |
| `AppUser`       | Email, FullName, Role, TenantId                                   |
| `TenantSettings`| DefaultLaborRate, DefaultMachineRate, OverheadPercent, TargetMargin|
| `Job`           | JobNumber, CustomerName, Description, Status                      |
| `JobEstimate`   | LaborHrs, MaterialCost, MachineHrs, Rates, QuotePrice, AIGenerated|
| `JobActuals`    | LaborHrs, MaterialCost, MachineHrs, Rates, ActualRevenue          |

---

## Definition of Done (Per Feature)

- [ ] Entity + EF Core configuration created
- [ ] Service interface + implementation
- [ ] Razor Page(s) with forms/display
- [ ] Role-based access enforced
- [ ] Tenant scoping verified
- [ ] Basic input validation
- [ ] At least 1 MSTest unit test for service logic
- [ ] Manual smoke test passed

---

## Risk Register

| Risk                              | Mitigation                                    |
|-----------------------------------|-----------------------------------------------|
| Claude API rate limits / downtime | Manual quote fallback always available         |
| 24-hour scope creep               | Strict cut lines defined above                 |
| SQLite concurrency limits         | Acceptable for demo; upgrade path to Postgres  |
| Seed data looks unrealistic       | Research real sheetmetal pricing ranges         |
| Auth complexity eats time         | Use Identity scaffolding, minimal custom UI    |

---

*Generated for MetalMetrics Hackathon Sprint — Ready to build! 🚀*
