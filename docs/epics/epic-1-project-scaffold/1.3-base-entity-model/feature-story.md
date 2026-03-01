# Feature 1.3 — Base Entity Model

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Complete

---

## User Story

**As a** developer,
**I want** a base entity class with common fields (Id, TenantId, timestamps),
**so that** all domain entities inherit consistent identity, tenancy, and auditing behavior.

---

## Implementation

### BaseEntity (`Core/Entities/BaseEntity.cs`)

```csharp
public abstract class BaseEntity : IAuditable
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### IAuditable (`Core/Interfaces/IAuditable.cs`)

```csharp
public interface IAuditable
{
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
}
```

### Auto-Behavior (AppDbContext.SaveChangesAsync)

```csharp
// Auto-set TenantId for new BaseEntity objects
foreach (var entry in ChangeTracker.Entries<BaseEntity>()
    .Where(e => e.State == EntityState.Added && e.Entity.TenantId == Guid.Empty))
{
    entry.Entity.TenantId = _tenantProvider.TenantId;
}

// Auto-set audit timestamps
foreach (var entry in ChangeTracker.Entries<IAuditable>())
{
    if (entry.State == EntityState.Added)
        entry.Entity.CreatedAt = DateTime.UtcNow;
    if (entry.State == EntityState.Modified)
        entry.Entity.UpdatedAt = DateTime.UtcNow;
}
```

### Entities extending BaseEntity

`Tenant`, `TenantSettings`, `Job`, `JobEstimate`, `JobActuals`, `JobAssignment`, `JobTimeEntry`, `JobNote`

Note: `AppUser` extends `IdentityUser` (not BaseEntity) but has its own `TenantId` and `Role` properties.

---

## Definition of Done

- [x] BaseEntity abstract class with Id (Guid), TenantId, CreatedAt, UpdatedAt
- [x] IAuditable interface defined
- [x] SaveChangesAsync auto-sets TenantId and timestamps
- [x] 3 unit tests (Guid generation, uniqueness, default TenantId)
