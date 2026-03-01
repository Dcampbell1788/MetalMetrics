# Feature 5.1 — Job Actuals Entity

**Epic:** Epic 5 — Actuals Entry
**Status:** Complete

---

## User Story

**As a** developer,
**I want** a `JobActuals` entity that stores real costs incurred on a job,
**so that** actual costs can be compared against estimates for profitability.

---

## Implementation

### JobActuals Entity (`Core/Entities/JobActuals.cs`)

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Inherited from BaseEntity |
| JobId | Guid | FK to Job |
| ActualLaborHours | decimal | Actual labor hours worked |
| LaborRate | decimal | Actual $/hr (may differ from estimate) |
| ActualMaterialCost | decimal | Actual material cost |
| ActualMachineHours | decimal | Actual machine hours |
| MachineRate | decimal | Actual $/hr |
| OverheadPercent | decimal | Actual overhead % applied |
| TotalActualCost | decimal | Computed by ActualsService |
| ActualRevenue | decimal | What was invoiced/collected |
| Notes | string | Free-text context |
| EnteredBy | string | User email who recorded actuals |

### Cost Calculation Formula (ActualsService.CalculateTotals)

Same formula as estimates:
```
Subtotal = (ActualLaborHours * LaborRate) + ActualMaterialCost + (ActualMachineHours * MachineRate)
Overhead = Subtotal * (OverheadPercent / 100)
TotalActualCost = Subtotal + Overhead
```

### IActualsService (`Core/Interfaces/IActualsService.cs`)

```csharp
Task<JobActuals?> GetByJobIdAsync(Guid jobId);
Task SaveAsync(JobActuals actuals);          // Upsert: update if exists, insert if new
JobActuals CalculateTotals(JobActuals actuals);
```

### Relationship

One-to-one with Job (`Job.Actuals`). Cascade delete configured in AppDbContext.

---

## Definition of Done

- [x] JobActuals entity with all cost fields
- [x] IActualsService with upsert Save and CalculateTotals
- [x] One-to-one with Job, cascade delete
- [x] Migration applied
- [x] 2 unit tests for cost calculation
