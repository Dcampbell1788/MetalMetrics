# Feature 3.1 — Tenant Entity

**Epic:** Epic 3 — Job & Quote Management (Core Data)
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Small

---

## User Story

**As a** developer,
**I want** a `Tenant` entity that represents a company in the system,
**so that** all data can be scoped to a specific company and users are correctly associated with their organization.

---

## Acceptance Criteria

- [ ] `Tenant` entity created in `MetalMetrics.Core` inheriting from `BaseEntity`
- [ ] `Tenant` includes: `CompanyName` (string, required)
- [ ] One-to-many relationship: `Tenant` → `AppUser` (a tenant has many users)
- [ ] One-to-many relationship: `Tenant` → `Job` (a tenant has many jobs)
- [ ] One-to-one relationship: `Tenant` → `TenantSettings`
- [ ] EF Core configuration via Fluent API (entity config class)
- [ ] Migration created and applied cleanly
- [ ] `CompanyName` is required and has a max length constraint

---

## Technical Notes

- Entity location: `MetalMetrics.Core/Entities/Tenant.cs`
- EF Config location: `MetalMetrics.Infrastructure/Data/Configurations/TenantConfiguration.cs`
- `Tenant` inherits `Id`, `TenantId`, `CreatedAt`, `UpdatedAt` from `BaseEntity`
  - Note: For the `Tenant` entity itself, `TenantId` can equal `Id` (self-referencing)
- Navigation properties:
  ```csharp
  public ICollection<AppUser> Users { get; set; }
  public ICollection<Job> Jobs { get; set; }
  public TenantSettings Settings { get; set; }
  ```

---

## Dependencies

- Feature 1.2 (EF Core + SQLite Setup)
- Feature 1.3 (Base Entity Model)

---

## Definition of Done

- [ ] `Tenant` entity created with correct properties and relationships
- [ ] Fluent API configuration applied
- [ ] Migration created and database updated successfully
- [ ] `CompanyName` validation enforced at the database level
