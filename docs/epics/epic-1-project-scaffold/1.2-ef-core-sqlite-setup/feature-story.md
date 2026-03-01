# Feature 1.2 — EF Core + SQLite Setup

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Complete

---

## User Story

**As a** developer,
**I want** Entity Framework Core configured with SQLite and multi-tenant support,
**so that** all data access is tenant-scoped and the database requires zero setup.

---

## Implementation

### AppDbContext (`Infrastructure/Data/AppDbContext.cs`)

Extends `IdentityDbContext<AppUser, IdentityRole, string>`.

**DbSets:** `Tenants`, `TenantSettings`, `Jobs`, `JobEstimates`, `JobActuals`, `JobAssignments`, `JobTimeEntries`, `JobNotes`

**SaveChangesAsync override behavior:**
1. New `BaseEntity` with empty `TenantId` -> auto-set from `ITenantProvider`
2. New `IAuditable` -> `CreatedAt = DateTime.UtcNow`
3. Modified `IAuditable` -> `UpdatedAt = DateTime.UtcNow`

**OnModelCreating:**
- Cascade deletes: `Tenant.Settings`, `Job.Estimate`, `Job.Actuals`, `Job.Assignments`, `Job.TimeEntries`, `Job.Notes`
- Unique indexes: `(TenantId, JobNumber)`, `(TenantId, Slug)` on Job entity

### ITenantProvider (`Core/Interfaces/ITenantProvider.cs`)

```csharp
public interface ITenantProvider
{
    Guid TenantId { get; }
}
```

### TenantProvider (`Infrastructure/Services/TenantProvider.cs`)

Reads `TenantId` claim from `HttpContext.User`. Returns `Guid.Empty` if unauthenticated.

### DesignTimeDbContextFactory (`Infrastructure/Data/DesignTimeDbContextFactory.cs`)

Enables `dotnet ef` commands with a `StubTenantProvider` that returns `Guid.Empty`.

### Connection String (`Web/appsettings.json`)

```json
{ "ConnectionStrings": { "DefaultConnection": "Data Source=metalmetrics.db" } }
```

### Auto-Startup (`Web/Program.cs`)

```csharp
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
context.Database.Migrate();  // Auto-apply all migrations
```

### SQLite Limitation

SQLite cannot apply aggregate `Sum()` on `decimal` expressions. Must materialize with `ToListAsync()` first, then use LINQ `.Sum()` client-side.

### Migrations (7 total, auto-applied on startup)

1. `InitialIdentityAndTenant` - Identity + Tenant + AppUser
2. `AddTenantSettings` - TenantSettings
3. `AddJobAndJobEstimate` - Job + JobEstimate
4. `AddJobActuals` - JobActuals
5. `AddJobAssignmentTimeEntryNotes` - JobAssignment, JobTimeEntry, JobNote
6. `AddTimeEntryUserNavigation` - FK for TimeEntry.User
7. `AddJobSlug` - Job.Slug column

---

## Definition of Done

- [x] EF Core SQLite packages installed and configured
- [x] AppDbContext with auto-audit and auto-TenantId
- [x] ITenantProvider interface and TenantProvider implementation
- [x] DesignTimeDbContextFactory for migrations
- [x] 7 migrations created and auto-applied
- [x] 3 unit tests for AppDbContext behavior
