# Feature 5.4 — Job Completion Workflow

**Epic:** Epic 5 — Actuals Entry
**Status:** Complete

---

## User Story

**As a** project manager,
**I want** a structured workflow for completing and invoicing jobs that ensures actuals are recorded,
**so that** no job is marked done without the cost data needed for profitability analysis.

---

## Implementation

### Status Flow

```
Quoted -> InProgress -> Completed -> Invoiced
```

### Status Transitions (Job Details Page — `Web/Pages/Jobs/Details.cshtml.cs`)

**OnPostStart** (Quoted -> InProgress):
- Sets `Job.Status = JobStatus.InProgress`
- No prerequisites

**OnPostComplete** (InProgress -> Completed):
- Validates actuals exist (`job.Actuals != null`)
- If no actuals: TempData error "Please enter actuals first", redirect back
- Sets `Job.Status = JobStatus.Completed`
- Sets `Job.CompletedAt = DateTime.UtcNow`

**OnPostInvoice** (Completed -> Invoiced):
- Sets `Job.Status = JobStatus.Invoiced`

### Validation Rules

- Backward transitions are not offered (no buttons to go backward)
- CompletedAt only set on first transition to Completed
- Actuals must exist before Completed status

### UI

Status-appropriate action buttons shown on Job Details page:
- Quoted job: "Start Job" button
- InProgress job: "Mark Complete" button (+ links to enter actuals)
- Completed job: "Mark Invoiced" button
- Invoiced job: no status actions

---

## Definition of Done

- [x] Status transitions work correctly (one-directional)
- [x] Actuals required before completion
- [x] CompletedAt set on completion
- [x] Action buttons shown per current status
- [x] 1 unit test for status transition logic (in JobServiceTests)
