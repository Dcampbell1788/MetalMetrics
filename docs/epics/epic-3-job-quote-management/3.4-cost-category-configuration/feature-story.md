# Feature 3.4 — Cost Category Configuration (Tenant-Level Rates)

**Epic:** Epic 3 — Job & Quote Management (Core Data)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Small

---

## User Story

**As an** admin or company owner,
**I want** to configure default labor rates, machine rates, and overhead percentages for my company,
**so that** quote forms are pre-populated with our standard rates and estimators don't have to enter them manually every time.

---

## Acceptance Criteria

- [ ] `TenantSettings` entity created with default rate fields
- [ ] One-to-one relationship: `Tenant` → `TenantSettings`
- [ ] **Settings page** (`/Admin/Settings`): form to view and edit tenant defaults
- [ ] Settings form includes: Default Labor Rate, Default Machine Rate, Default Overhead Percent
- [ ] Saving settings persists values to the database
- [ ] Quote forms (Feature 3.3) pre-populate rates from these settings
- [ ] Default values are created when a new tenant registers
- [ ] Access: Admin and Owner roles only

---

## Entity Fields

| Field                    | Type    | Notes                               |
|--------------------------|---------|--------------------------------------|
| `Id`                     | Guid    | Inherited from `BaseEntity`          |
| `TenantId`               | Guid    | FK to `Tenant` (unique)              |
| `DefaultLaborRate`       | decimal | Default $/hour for labor             |
| `DefaultMachineRate`     | decimal | Default $/hour for machine time      |
| `DefaultOverheadPercent` | decimal | Default overhead % (e.g., 15)        |
| `TargetMarginPercent`    | decimal | Target profit margin % (e.g., 20)    |

---

## Technical Notes

- Entity: `MetalMetrics.Core/Entities/TenantSettings.cs`
- Page: `Pages/Admin/Settings.cshtml` + `Settings.cshtml.cs`
- Create default `TenantSettings` during tenant registration (Feature 2.2) with reasonable defaults:
  - Labor Rate: $75/hr
  - Machine Rate: $150/hr
  - Overhead: 15%
  - Target Margin: 20%
- Use `[Authorize(Policy = "AdminOnly")]` on the settings page
- Service: consider an `ITenantSettingsService` or include in `ITenantService`

---

## Dependencies

- Feature 3.1 (Tenant Entity)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] `TenantSettings` entity created with all fields
- [ ] Settings page renders and saves correctly
- [ ] Default settings created on tenant registration
- [ ] Quote forms consume these defaults
- [ ] Page restricted to Admin/Owner roles
- [ ] Manual smoke test passed
