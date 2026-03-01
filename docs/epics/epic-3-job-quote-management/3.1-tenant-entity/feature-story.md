# Feature 3.1 — Tenant Entity

**Epic:** Epic 3 — Job & Quote Management
**Status:** Complete

---

## User Story

**As a** developer,
**I want** a `Tenant` entity that represents a company,
**so that** all data is scoped to a specific company.

---

## Implementation

### Tenant Entity (`Core/Entities/Tenant.cs`)

```csharp
public class Tenant : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;
    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public TenantSettings? Settings { get; set; }
}
```

### Relationships (AppDbContext OnModelCreating)

- `Tenant` -> `TenantSettings`: One-to-one, cascade delete
- `Tenant` -> `AppUser`: One-to-many (via AppUser.TenantId)
- `Tenant` -> `Job`: One-to-many (via Job.TenantId)

### Note on TenantId

For the `Tenant` entity itself, `TenantId` is set to equal `Id` (self-referencing) by the `SaveChangesAsync` auto-assign logic.

---

## Definition of Done

- [x] Tenant entity with CompanyName and navigation properties
- [x] One-to-one with TenantSettings (cascade delete)
- [x] One-to-many with AppUser and Job
- [x] Migration applied
