# Feature 1.3 — Base Entity Model

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Pending
**Priority:** Critical (Foundation)
**Estimated Effort:** Small

---

## User Story

**As a** developer,
**I want** a base entity class with common fields (Id, TenantId, timestamps),
**so that** all domain entities inherit consistent identity, tenancy, and auditing behavior without duplication.

---

## Acceptance Criteria

- [ ] `BaseEntity` abstract class created in `MetalMetrics.Core`
- [ ] `BaseEntity` includes: `Id` (Guid), `TenantId` (Guid), `CreatedAt` (DateTime), `UpdatedAt` (DateTime)
- [ ] `IAuditable` interface defined with `CreatedAt` and `UpdatedAt` properties
- [ ] `BaseEntity` implements `IAuditable`
- [ ] `SaveChangesAsync` override in `AppDbContext` automatically sets `CreatedAt` on insert and `UpdatedAt` on update
- [ ] `Id` defaults to `Guid.NewGuid()` on creation
- [ ] `TenantId` is required (non-nullable) on all entities

---

## Technical Notes

- `BaseEntity` location: `MetalMetrics.Core/Entities/BaseEntity.cs`
- `IAuditable` location: `MetalMetrics.Core/Interfaces/IAuditable.cs`
- `SaveChangesAsync` override pattern:
  ```csharp
  foreach (var entry in ChangeTracker.Entries<IAuditable>())
  {
      if (entry.State == EntityState.Added)
          entry.Entity.CreatedAt = DateTime.UtcNow;
      if (entry.State == EntityState.Modified)
          entry.Entity.UpdatedAt = DateTime.UtcNow;
  }
  ```
- All future entities (Job, JobEstimate, JobActuals, Tenant, TenantSettings) will inherit from `BaseEntity`

---

## Dependencies

- Feature 1.1 (Solution & Project Structure — Core project must exist)

---

## Definition of Done

- [ ] `BaseEntity` abstract class created with all required fields
- [ ] `IAuditable` interface defined
- [ ] `SaveChangesAsync` override auto-populates audit timestamps
- [ ] At least 1 unit test verifying audit field behavior
- [ ] All fields have appropriate data annotations or Fluent API configuration
