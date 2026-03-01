# Feature 3.3 — Quote / Estimate Entity

**Epic:** Epic 3 — Job & Quote Management
**Status:** Complete

---

## User Story

**As an** estimator or project manager,
**I want** to create detailed cost estimates broken down by labor, material, machine, and overhead,
**so that** I can generate accurate quotes and track AI-assisted estimation.

---

## Implementation

### JobEstimate Entity (`Core/Entities/JobEstimate.cs`)

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Inherited from BaseEntity |
| JobId | Guid | FK to Job |
| EstimatedLaborHours | decimal | Total labor hours |
| LaborRate | decimal | $/hour |
| EstimatedMaterialCost | decimal | Total material cost |
| EstimatedMachineHours | decimal | Machine hours |
| MachineRate | decimal | $/hour |
| OverheadPercent | decimal | Applied to subtotal |
| TotalEstimatedCost | decimal | Computed by service |
| QuotePrice | decimal | Price to customer |
| EstimatedMarginPercent | decimal | Computed by service |
| AIGenerated | bool | Created via Claude AI? |
| AIPromptSnapshot | string? | JSON of AI prompt + response |
| CreatedBy | string | User email |

### Cost Calculation Formula (QuoteService.CalculateTotals)

```
Subtotal = (LaborHours * LaborRate) + MaterialCost + (MachineHours * MachineRate)
Overhead = Subtotal * (OverheadPercent / 100)
TotalEstimatedCost = Subtotal + Overhead
EstimatedMarginPercent = QuotePrice > 0 ? (QuotePrice - TotalCost) / QuotePrice * 100 : 0
```

### IQuoteService (`Core/Interfaces/IQuoteService.cs`)

```csharp
Task<JobEstimate?> GetByJobIdAsync(Guid jobId);
Task<JobEstimate> CreateAsync(JobEstimate estimate);
JobEstimate CalculateTotals(JobEstimate estimate);
```

### Pages

| Page | Path | Purpose |
|------|------|---------|
| Manual Quote | `/Jobs/Quote/Create/{slug}` | Form with all cost fields, pre-populated from TenantSettings |
| AI Quote | `/Jobs/Quote/AI/{slug}` | Structured form for Claude API input |
| AI Review | `/Jobs/Quote/Review/{slug}` | Side-by-side AI suggestion vs editable form |
| View Quote | `/Jobs/Quote/View/{slug}` | Read-only estimate display |

### Rate Pre-Population

Quote forms load defaults from `TenantSettings`: `DefaultLaborRate`, `DefaultMachineRate`, `DefaultOverheadPercent`.

---

## Definition of Done

- [x] JobEstimate entity with all cost breakdown fields
- [x] IQuoteService with CalculateTotals formula
- [x] Create, View, AI, Review pages working
- [x] AIGenerated flag and AIPromptSnapshot for audit
- [x] Default rates from TenantSettings
- [x] 2 unit tests for cost calculation
