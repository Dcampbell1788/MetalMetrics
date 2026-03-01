# Feature 1.1 — Solution & Project Structure

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Complete
**Priority:** Critical (Foundation)

---

## User Story

**As a** developer on the MetalMetrics team,
**I want** a well-organized .NET 8 solution with clearly separated projects,
**so that** the codebase is maintainable, testable, and follows clean architecture principles.

---

## Implementation

### Solution Structure

```
MetalMetrics.sln
├── MetalMetrics.Core/              (net8.0 classlib)
│   ├── Entities/                   BaseEntity, AppUser, Tenant, TenantSettings, Job, JobEstimate, JobActuals, JobAssignment, JobTimeEntry, JobNote
│   ├── DTOs/                       AIQuoteRequest/Response, JobSummaryDto, DashboardKpiDto, JobProfitabilityReport, VarianceDetail, CustomerProfitabilityDto, CategoryVarianceDto, AtRiskJobDto
│   ├── Enums/                      AppRole, JobStatus
│   └── Interfaces/                 ITenantProvider, IJobService, IQuoteService, IActualsService, IProfitabilityService, IDashboardService, IReportsService, IAIQuoteService, IJobAssignmentService, ITimeEntryService, IJobNoteService, IAuditable
├── MetalMetrics.Infrastructure/    (net8.0 classlib, refs Core)
│   ├── Data/                       AppDbContext, DbSeeder, DesignTimeDbContextFactory
│   ├── Migrations/                 7 EF Core migrations
│   └── Services/                   JobService, QuoteService, ActualsService, ProfitabilityService, DashboardService, ReportsService, ClaudeAIQuoteService, PromptTemplates, JobAssignmentService, TimeEntryService, JobNoteService, TenantProvider
├── MetalMetrics.Web/               (net8.0 webapp, refs Core + Infrastructure)
│   ├── Pages/                      Razor Pages (Dashboard, Jobs, Admin, Reports, Auth)
│   ├── wwwroot/                    css/site.css, uploads/notes/
│   └── Program.cs                  DI, auth, middleware, seeding
└── MetalMetrics.Tests/             (net8.0 mstest, refs all)
    ├── Core/                       BaseEntityTests
    └── Infrastructure/             AppDbContextTests, JobServiceTests, QuoteServiceTests, ActualsServiceTests, ProfitabilityServiceTests
```

### Project References

- `Web` -> `Core` + `Infrastructure`
- `Infrastructure` -> `Core`
- `Tests` -> `Core` + `Infrastructure` + `Web`

### NuGet Dependencies

**Core:** `Microsoft.Extensions.Identity.Stores 8.0.*`
**Infrastructure:** `Microsoft.AspNetCore.Identity.EntityFrameworkCore 8.0.*`, `Microsoft.EntityFrameworkCore.Sqlite 8.0.*`, `Microsoft.EntityFrameworkCore.Tools 8.0.*`
**Web:** `Microsoft.EntityFrameworkCore.Design 8.0.*`
**Tests:** `MSTest 3.1.1`, `Microsoft.NET.Test.Sdk 17.8.0`, `Microsoft.EntityFrameworkCore.InMemory 8.0.*`

---

## Technical Notes

- All projects target `net8.0` with nullable reference types enabled
- No `.editorconfig` or `global.json` — uses SDK defaults
- Solution builds with `dotnet build` — zero errors, zero warnings
- Web project has `UserSecretsId` for local Claude API key storage

---

## Definition of Done

- [x] All four projects created and referenced correctly
- [x] Solution builds with zero errors and zero warnings
- [x] Project references follow clean architecture
- [x] 20 unit tests passing
