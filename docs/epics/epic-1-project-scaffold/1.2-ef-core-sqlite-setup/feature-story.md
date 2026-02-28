# Feature 1.2 — EF Core + SQLite Setup

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Pending
**Priority:** Critical (Foundation)
**Estimated Effort:** Medium

---

## User Story

**As a** developer,
**I want** Entity Framework Core configured with SQLite and multi-tenant query filtering,
**so that** all data access is tenant-scoped by default and the database is ready for entity development.

---

## Acceptance Criteria

- [ ] EF Core SQLite NuGet packages installed in `MetalMetrics.Infrastructure`
- [ ] `AppDbContext` class created with `DbSet` properties for core entities
- [ ] Global query filter on `AppDbContext` automatically scopes all queries by `TenantId`
- [ ] `ITenantProvider` interface defined in `MetalMetrics.Core`
- [ ] `TenantProvider` implementation created in `MetalMetrics.Infrastructure` (reads tenant from authenticated user claims)
- [ ] Initial EF Core migration created and applies cleanly
- [ ] Database seeding script creates default/demo data
- [ ] Connection string configured in `appsettings.json` (SQLite file path)
- [ ] `AppDbContext` registered in DI container in `Program.cs`

---

## Technical Notes

- NuGet packages: `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Microsoft.EntityFrameworkCore.Design`
- Global query filter pattern:
  ```csharp
  modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantProvider.TenantId);
  ```
- SQLite database file stored in project root or `App_Data` folder
- Use `IDesignTimeDbContextFactory` for migrations support
- Consider `HasData()` for seed data or a separate `DbSeeder` service

---

## Dependencies

- Feature 1.1 (Solution & Project Structure)
- Feature 1.3 (Base Entity Model — for `TenantId` on `BaseEntity`)

---

## Definition of Done

- [ ] EF Core packages installed and configured
- [ ] `AppDbContext` with global tenant filter working
- [ ] `ITenantProvider` interface and implementation created
- [ ] Initial migration created successfully
- [ ] Database can be created/updated via `dotnet ef database update`
- [ ] Connection string externalized to `appsettings.json`
- [ ] At least 1 unit test for tenant filtering logic
