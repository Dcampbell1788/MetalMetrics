# Feature 5.4 — Job Completion Workflow

**Epic:** Epic 5 — Actuals Entry (Post-Job Tracking)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** project manager,
**I want** a structured workflow for completing and invoicing a job that ensures actuals are recorded,
**so that** no job is marked as done without capturing the real costs needed for profitability analysis.

---

## Acceptance Criteria

- [ ] "Mark as Completed" action available on the Job Details page
- [ ] Completion action only available when job status is `InProgress`
- [ ] If actuals have not been entered, prompts the user to enter actuals first
- [ ] If actuals exist, sets `Job.Status` to `Completed` and `Job.CompletedAt` to current time
- [ ] "Mark as Invoiced" action available for `Completed` jobs
- [ ] Invoiced status requires actuals to exist (validates before status change)
- [ ] Status transitions are one-directional: Quoted → InProgress → Completed → Invoiced
- [ ] Status change is logged with timestamp
- [ ] Access: Admin, Owner, ProjectManager

---

## Status Flow

```
┌─────────┐    ┌────────────┐    ┌───────────┐    ┌──────────┐
│ Quoted   │ →  │ InProgress │ →  │ Completed │ →  │ Invoiced │
└─────────┘    └────────────┘    └───────────┘    └──────────┘
                                      ▲
                                      │
                              Requires actuals
                              to be entered
```

---

## Technical Notes

- Actions can be buttons on `/Jobs/Details/{id}` or separate confirmation pages
- Service method: `IJobService.UpdateStatusAsync(jobId, newStatus)`
- Validation logic:
  ```csharp
  if (newStatus == JobStatus.Completed && !job.HasActuals)
      return Error("Please enter actuals before completing this job.");
  if (newStatus == JobStatus.Invoiced && !job.HasActuals)
      return Error("Actuals required before marking as invoiced.");
  ```
- Prompt for actuals: redirect to `/Jobs/{jobId}/Actuals/Enter` if missing
- Set `CompletedAt` only on transition to `Completed`
- Consider a confirmation dialog before status changes
- Prevent backward status transitions (Completed → InProgress not allowed)

---

## Dependencies

- Feature 3.2 (Job Entity — status field)
- Feature 5.1 (Job Actuals Entity — for existence check)
- Feature 5.2 (Actuals Entry Form — redirect target)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] "Mark as Completed" and "Mark as Invoiced" actions work correctly
- [ ] Status transitions enforce the correct flow
- [ ] Missing actuals prompts the user to enter them
- [ ] `CompletedAt` is set on completion
- [ ] Backward transitions are prevented
- [ ] Role-based access enforced
- [ ] At least 1 unit test for status transition logic
- [ ] Manual smoke test of the full workflow
