# Feature 5.3 — Quick Entry Mode (Journeyman Optimized)

**Epic:** Epic 5 — Actuals Entry (Post-Job Tracking)
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Small

---

## User Story

**As a** journeyman on the shop floor,
**I want** a simplified, mobile-friendly form to quickly log my actual hours and material usage,
**so that** I can record job costs without navigating a complex interface on my phone.

---

## Acceptance Criteria

- [ ] Quick entry page accessible at `/Jobs/{jobId}/Actuals/Quick`
- [ ] Simplified form with only essential fields: Labor Hours, Material Cost, Machine Hours, Notes
- [ ] Rates auto-filled from `TenantSettings` defaults (not editable in quick mode)
- [ ] Large input fields and buttons optimized for mobile/touch
- [ ] Minimal UI — no side-by-side comparison, no variance display
- [ ] Save button creates/updates `JobActuals` with auto-filled rates
- [ ] Success confirmation with option to enter actuals for another job
- [ ] Access: All roles that can enter actuals (Admin, Owner, PM, Foreman, Journeyman)

---

## Page Layout (Mobile-First)

```
┌───────────────────────────┐
│  Quick Entry              │
│  Job #JOB-0042            │
│  ABC Fabrication          │
│                           │
│  Labor Hours              │
│  ┌───────────────────┐    │
│  │ 5.0               │    │
│  └───────────────────┘    │
│                           │
│  Material Cost ($)        │
│  ┌───────────────────┐    │
│  │ 310.00             │    │
│  └───────────────────┘    │
│                           │
│  Machine Hours            │
│  ┌───────────────────┐    │
│  │ 1.5               │    │
│  └───────────────────┘    │
│                           │
│  Notes                    │
│  ┌───────────────────┐    │
│  │                   │    │
│  │                   │    │
│  └───────────────────┘    │
│                           │
│  ┌───────────────────┐    │
│  │    SAVE ACTUALS   │    │
│  └───────────────────┘    │
└───────────────────────────┘
```

---

## Technical Notes

- Page: `Pages/Jobs/Actuals/Quick.cshtml` + `Quick.cshtml.cs`
- Auto-fill from `TenantSettings`:
  - `LaborRate` = `DefaultLaborRate`
  - `MachineRate` = `DefaultMachineRate`
  - `OverheadPercent` = `DefaultOverheadPercent`
- Reuses `IActualsService.SaveActualsAsync()` from Feature 5.2
- Mobile CSS considerations:
  - Large font sizes for inputs (16px+ to prevent iOS zoom)
  - Full-width inputs and buttons
  - Minimal padding/margins
  - Number input type for numeric fields (shows numeric keyboard)
- Consider linking to this from the job list for quick access

---

## Dependencies

- Feature 5.1 (Job Actuals Entity)
- Feature 3.4 (Tenant Settings — for auto-filling rates)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] Quick entry page renders with simplified form
- [ ] Rates auto-filled from tenant settings
- [ ] Save creates/updates `JobActuals` correctly
- [ ] Mobile-responsive layout works on phone viewport
- [ ] Input fields are touch-friendly (large targets)
- [ ] Role-based access enforced
- [ ] Manual smoke test on mobile viewport
