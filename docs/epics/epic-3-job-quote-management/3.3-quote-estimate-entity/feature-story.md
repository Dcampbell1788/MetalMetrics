# Feature 3.3 — Quote / Estimate Entity

**Epic:** Epic 3 — Job & Quote Management (Core Data)
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Large

---

## User Story

**As an** estimator or project manager,
**I want** to create detailed cost estimates for a job broken down by labor, material, machine, and overhead,
**so that** I can generate accurate quotes and track whether the AI assisted in creating the estimate.

---

## Acceptance Criteria

- [ ] `JobEstimate` entity created with all cost breakdown fields
- [ ] One-to-one relationship: `Job` → `JobEstimate`
- [ ] **Create Quote page** (`/Jobs/{jobId}/Quote/Create`): form with all estimate fields
- [ ] **View Quote page** (`/Jobs/{jobId}/Quote/View`): read-only display of estimate
- [ ] `TotalEstimatedCost` is computed (labor + material + machine + overhead)
- [ ] `EstimatedMarginPercent` is computed from `QuotePrice` vs `TotalEstimatedCost`
- [ ] Form pre-populates rates from `TenantSettings` defaults
- [ ] `AIGenerated` flag tracks if the estimate was created via AI
- [ ] `AIPromptSnapshot` stores the AI prompt/response JSON for auditing
- [ ] Access: Admin, Owner, ProjectManager, Estimator

---

## Entity Fields

| Field                    | Type      | Notes                                      |
|--------------------------|-----------|--------------------------------------------|
| `Id`                     | Guid      | Inherited from `BaseEntity`                |
| `JobId`                  | Guid      | FK to `Job`                                |
| `TenantId`               | Guid      | Inherited from `BaseEntity`                |
| `EstimatedLaborHours`    | decimal   | Total labor hours estimated                |
| `LaborRate`              | decimal   | $/hour for labor                           |
| `EstimatedMaterialCost`  | decimal   | Total material cost (sheet/plate)          |
| `EstimatedMachineHours`  | decimal   | Machine time in hours                      |
| `MachineRate`            | decimal   | $/hour for machine time                    |
| `OverheadPercent`        | decimal   | Overhead % applied to subtotal             |
| `TotalEstimatedCost`     | decimal   | Computed: labor + material + machine + OH  |
| `QuotePrice`             | decimal   | Price quoted to customer                   |
| `EstimatedMarginPercent` | decimal   | Computed: (quote - cost) / quote * 100     |
| `AIGenerated`            | bool      | Was this created via AI?                   |
| `AIPromptSnapshot`       | string    | JSON blob of AI prompt and response        |
| `CreatedBy`              | string    | User who created the estimate              |
| `CreatedAt`              | DateTime  | Inherited from `BaseEntity`                |

---

## Technical Notes

- Entity: `MetalMetrics.Core/Entities/JobEstimate.cs`
- Service: `IQuoteService` / `QuoteService`
- Computed fields can be calculated in the service layer before saving, or as C# computed properties
- `TotalEstimatedCost` formula:
  ```
  (LaborHours * LaborRate) + MaterialCost + (MachineHours * MachineRate) + Overhead
  Overhead = OverheadPercent / 100 * (Labor + Material + Machine subtotal)
  ```
- Pre-populate `LaborRate`, `MachineRate`, `OverheadPercent` from `TenantSettings` (Feature 3.4)
- `AIPromptSnapshot` stores JSON — use `string` type with JSON serialization

---

## Dependencies

- Feature 3.2 (Job Entity — FK to Job)
- Feature 3.4 (Cost Category Configuration — for default rate pre-population)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] `JobEstimate` entity with all fields and relationships configured
- [ ] `IQuoteService` interface and `QuoteService` implementation
- [ ] Create and View quote pages render correctly
- [ ] Computed fields calculate correctly
- [ ] Default rates pre-populate from `TenantSettings`
- [ ] `AIGenerated` and `AIPromptSnapshot` fields work correctly
- [ ] Tenant scoping verified
- [ ] Role-based access enforced
- [ ] At least 1 unit test for cost calculation logic
- [ ] Manual smoke test passed
