# Feature 5.3 — Quick Entry Mode

**Epic:** Epic 5 — Actuals Entry
**Status:** Complete

---

## User Story

**As a** journeyman on the shop floor,
**I want** a simplified mobile-friendly form to quickly log hours and material,
**so that** I can record costs without navigating a complex interface.

---

## Implementation

### Page: `/Jobs/Actuals/Quick/{slug}` (`Web/Pages/Jobs/Actuals/Quick.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "CanEnterActuals")]`

### Simplified Form Fields

Only 3 cost fields + notes:
- Labor Hours
- Material Cost ($)
- Machine Hours
- Notes

### Auto-Filled Values (from TenantSettings or JobEstimate)

- `LaborRate` = from estimate, or TenantSettings.DefaultLaborRate
- `MachineRate` = from estimate, or TenantSettings.DefaultMachineRate
- `OverheadPercent` = from estimate, or TenantSettings.DefaultOverheadPercent
- `ActualRevenue` = from estimate QuotePrice (if available)

### OnPostAsync Flow

1. Build `JobActuals` from simplified form + auto-filled rates
2. Calculate totals via `ActualsService.CalculateTotals()`
3. Set `EnteredBy`
4. Upsert via `ActualsService.SaveAsync()`
5. Redirect to Job Details

### Mobile Considerations

- Large input fields
- Full-width buttons
- Numeric input types (shows numeric keyboard on mobile)
- Minimal UI — no side-by-side comparison

---

## Definition of Done

- [x] Simplified form with 3 cost fields + notes
- [x] Rates auto-filled from estimate/TenantSettings
- [x] Save creates/updates JobActuals correctly
- [x] Mobile-friendly layout
- [x] Role-based access enforced
