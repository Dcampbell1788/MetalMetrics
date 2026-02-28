# Feature 5.1 — Job Actuals Entity

**Epic:** Epic 5 — Actuals Entry (Post-Job Tracking)
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Small

---

## User Story

**As a** developer,
**I want** a `JobActuals` entity that stores the real costs incurred on a completed job,
**so that** actual costs can be compared against estimates to determine profitability.

---

## Acceptance Criteria

- [ ] `JobActuals` entity created in `MetalMetrics.Core` inheriting from `BaseEntity`
- [ ] One-to-one relationship: `Job` → `JobActuals`
- [ ] All cost fields mirror `JobEstimate` structure for direct comparison
- [ ] `TotalActualCost` is computed from actual values
- [ ] `ActualRevenue` captures what was actually invoiced/collected
- [ ] `Notes` field for free-text context about the actuals
- [ ] `EnteredBy` tracks which user recorded the actuals
- [ ] EF Core configuration via Fluent API
- [ ] Migration created and applied cleanly

---

## Entity Fields

| Field                | Type      | Notes                                     |
|----------------------|-----------|-------------------------------------------|
| `Id`                 | Guid      | Inherited from `BaseEntity`               |
| `JobId`              | Guid      | FK to `Job`                               |
| `TenantId`           | Guid      | Inherited from `BaseEntity`               |
| `ActualLaborHours`   | decimal   | Actual labor hours worked                 |
| `LaborRate`          | decimal   | Actual labor rate (may differ from est.)  |
| `ActualMaterialCost` | decimal   | Actual material cost                      |
| `ActualMachineHours` | decimal   | Actual machine time used                  |
| `MachineRate`        | decimal   | Actual machine rate                       |
| `OverheadPercent`    | decimal   | Actual overhead percentage applied        |
| `TotalActualCost`    | decimal   | Computed total actual cost                |
| `ActualRevenue`      | decimal   | What was invoiced/collected from customer |
| `Notes`              | string    | Free-text notes about the actuals         |
| `EnteredBy`          | string    | User who recorded the actuals             |
| `CreatedAt`          | DateTime  | Inherited from `BaseEntity`               |
| `UpdatedAt`          | DateTime  | Inherited from `BaseEntity`               |

---

## Technical Notes

- Entity: `MetalMetrics.Core/Entities/JobActuals.cs`
- Config: `MetalMetrics.Infrastructure/Data/Configurations/JobActualsConfiguration.cs`
- `TotalActualCost` formula matches `JobEstimate`:
  ```
  (ActualLaborHours * LaborRate) + ActualMaterialCost + (ActualMachineHours * MachineRate) + Overhead
  ```
- Navigation: `Job.Actuals` property for easy access
- Service: `IActualsService` / `ActualsService`

---

## Dependencies

- Feature 1.2 (EF Core + SQLite)
- Feature 1.3 (Base Entity Model)
- Feature 3.2 (Job Entity — FK relationship)

---

## Definition of Done

- [ ] `JobActuals` entity created with all fields and relationships
- [ ] Fluent API configuration applied
- [ ] Migration created and database updated
- [ ] `TotalActualCost` computed correctly
- [ ] At least 1 unit test for cost computation
