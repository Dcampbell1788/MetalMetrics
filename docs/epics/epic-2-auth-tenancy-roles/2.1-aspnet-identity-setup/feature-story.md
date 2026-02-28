# Feature 2.1 — ASP.NET Identity Setup

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Pending
**Priority:** Critical (Foundation)
**Estimated Effort:** Medium

---

## User Story

**As a** developer,
**I want** ASP.NET Identity configured with a custom `AppUser` that includes tenant and role information,
**so that** user authentication and tenant association are handled by the framework with minimal custom code.

---

## Acceptance Criteria

- [ ] `AppUser` class extends `IdentityUser` with additional properties: `TenantId` (Guid), `Role` (string/enum), `FullName` (string)
- [ ] ASP.NET Identity configured to use SQLite via `AppDbContext`
- [ ] Cookie authentication scheme configured
- [ ] Identity services registered in `Program.cs` DI container
- [ ] Identity tables created via EF Core migration
- [ ] Password policy configured (reasonable defaults for hackathon — not overly strict)
- [ ] `AppUser.TenantId` is indexed for query performance

---

## Technical Notes

- Extend `IdentityUser` in `MetalMetrics.Core/Entities/AppUser.cs`
- Register Identity in `Program.cs`:
  ```csharp
  builder.Services.AddDefaultIdentity<AppUser>()
      .AddRoles<IdentityRole>()
      .AddEntityFrameworkStores<AppDbContext>();
  ```
- Cookie config: redirect to `/Login` on unauthorized, `/AccessDenied` on forbidden
- Role enum values: `Admin`, `Owner`, `ProjectManager`, `Foreman`, `Estimator`, `Journeyman`
- Consider storing role as a claim for easy access in `TenantProvider` and nav

---

## Dependencies

- Feature 1.1 (Solution structure)
- Feature 1.2 (EF Core + SQLite — DbContext must exist)
- Feature 1.3 (Base Entity Model)

---

## Definition of Done

- [ ] `AppUser` entity created with `TenantId`, `Role`, `FullName`
- [ ] Identity configured with SQLite store
- [ ] Cookie auth scheme working (redirects on unauthorized)
- [ ] Identity migration applied cleanly
- [ ] Registration and login endpoints functional (even if UI is minimal)
