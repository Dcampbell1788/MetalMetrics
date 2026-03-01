# Feature 4.2 — Structured Quote Request Form

**Epic:** Epic 4 — AI-Powered Quoting
**Status:** Complete

---

## User Story

**As an** estimator,
**I want** to fill out a structured form describing the fabrication work,
**so that** the AI can generate an accurate cost estimate.

---

## Implementation

### Page: `/Jobs/Quote/AI/{slug}` (`Web/Pages/Jobs/Quote/AI.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanQuote")]`

### AIQuoteRequest DTO (`Core/DTOs/AIQuoteRequest.cs`)

| Field | Type | Options |
|-------|------|---------|
| MaterialType | string | Mild Steel, Stainless Steel, Aluminum, Galvanized, Copper, Other |
| MaterialThickness | string | Gauge or decimal (e.g., "16ga", "0.060") |
| PartDimensions | string | L x W (e.g., "24 x 48") |
| SheetSize | string | Optional full sheet size |
| Quantity | int | Number of parts |
| Operations | string | Comma-separated: Laser Cut, Brake/Bend, Punch, Weld, Deburr, Roll, Shear, Assembly |
| Complexity | string | Simple, Moderate, Complex |
| SpecialNotes | string? | Free text |
| LaborRate | decimal | From TenantSettings |
| MachineRate | decimal | From TenantSettings |
| OverheadPercent | decimal | From TenantSettings |

### Form Layout

- Material type: dropdown
- Operations: multi-select checkboxes
- Complexity: radio buttons
- Shop rates auto-populated from TenantSettings (hidden or read-only)
- Loading spinner on submit (button disabled during API call)

### OnPostAsync Flow

1. Build `AIQuoteRequest` from form values + TenantSettings rates
2. Call `_aiQuoteService.GenerateQuoteAsync(request)`
3. On success: serialize response + prompt snapshot to TempData, redirect to Review page
4. On failure: display error message with retry option
5. Manual quote fallback: link to `/Jobs/Quote/Create/{slug}`

---

## Definition of Done

- [x] AI Quote form with all fields (material, ops, complexity, notes)
- [x] Client + server validation
- [x] Loading spinner during API call
- [x] TempData pass to Review page on success
- [x] Error handling with retry/manual fallback
- [x] Role-based access enforced
