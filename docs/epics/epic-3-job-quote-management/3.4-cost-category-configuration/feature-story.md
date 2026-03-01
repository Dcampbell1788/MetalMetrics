# Feature 3.4 — Cost Category Configuration (Tenant-Level Rates)

**Epic:** Epic 3 — Job & Quote Management
**Status:** Complete

---

## User Story

**As an** admin or owner,
**I want** to configure default labor rates, machine rates, and overhead for my company,
**so that** quote forms are pre-populated with our standard rates.

---

## Implementation

### TenantSettings Entity (`Core/Entities/TenantSettings.cs`)

```csharp
public class TenantSettings : BaseEntity
{
    public decimal DefaultLaborRate { get; set; }
    public decimal DefaultMachineRate { get; set; }
    public decimal DefaultOverheadPercent { get; set; }
    public decimal TargetMarginPercent { get; set; }
}
```

### Relationship

One-to-one with Tenant (cascade delete). Configured in AppDbContext OnModelCreating.

### Admin Settings Page (`Web/Pages/Admin/Settings.cshtml.cs`)

- Authorization: `[Authorize(Policy = "AdminOnly")]`
- Form fields: Default Labor Rate, Default Machine Rate, Default Overhead %, Target Margin %
- Loads existing TenantSettings, updates on POST

### Default Values (created during registration)

Created in `Register.cshtml.cs` alongside the Tenant:
```csharp
new TenantSettings
{
    DefaultLaborRate = 75m,
    DefaultMachineRate = 150m,
    DefaultOverheadPercent = 15m,
    TargetMarginPercent = 20m
}
```

### Demo Seed Data Values

| Setting | Precision Metal Works | Budget Fabricators |
|---------|----------------------|-------------------|
| Labor Rate | $95/hr | $68/hr |
| Machine Rate | $175/hr | $130/hr |
| Overhead % | 30% | 20% |
| Target Margin % | 25% | 15% |

### Usage

- Quote forms (Create, AI Review) pre-populate rates from TenantSettings
- Quick Actuals entry uses rates for calculation
- Dashboard margin alerts reference TargetMarginPercent
- Profitability warnings reference TargetMarginPercent

---

## Definition of Done

- [x] TenantSettings entity with all rate fields
- [x] One-to-one with Tenant
- [x] Admin/Settings page for editing
- [x] Defaults created on registration
- [x] Quote/actuals forms consume these defaults
