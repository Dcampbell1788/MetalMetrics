# Feature 3.2 — Job Entity & CRUD

**Epic:** Epic 3 — Job & Quote Management (Core Data)
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Large

---

## User Story

**As a** project manager or admin,
**I want** to create, view, edit, and manage jobs with status tracking,
**so that** I can organize all fabrication work and track each job through its lifecycle.

---

## Acceptance Criteria

- [ ] `Job` entity created with all required fields
- [ ] `JobStatus` enum: `Quoted`, `InProgress`, `Completed`, `Invoiced`
- [ ] `JobNumber` auto-generated (e.g., `JOB-0001`, incrementing per tenant)
- [ ] **Job List page** (`/Jobs/Index`): displays all jobs for the current tenant
- [ ] Job list supports search by customer name or job number
- [ ] Job list supports filtering by status
- [ ] **Create Job page** (`/Jobs/Create`): form to create a new job
- [ ] **Job Details page** (`/Jobs/Details/{id}`): read-only view of job info
- [ ] **Edit Job page** (`/Jobs/Edit/{id}`): form to update job fields
- [ ] All pages are tenant-scoped (users only see their company's jobs)
- [ ] Access control: Admin, Owner, PM, Foreman have full access; Estimator and Journeyman have read-only

---

## Entity Fields

| Field          | Type       | Notes                                    |
|----------------|------------|------------------------------------------|
| `Id`           | Guid       | Inherited from `BaseEntity`              |
| `TenantId`     | Guid       | Inherited from `BaseEntity`              |
| `JobNumber`    | string     | Auto-generated, unique per tenant        |
| `CustomerName` | string     | Required                                 |
| `Description`  | string     | Optional, free text                      |
| `Status`       | JobStatus  | Enum, defaults to `Quoted`               |
| `CreatedAt`    | DateTime   | Inherited from `BaseEntity`              |
| `UpdatedAt`    | DateTime   | Inherited from `BaseEntity`              |
| `CompletedAt`  | DateTime?  | Set when status changes to `Completed`   |

---

## Technical Notes

- Entity: `MetalMetrics.Core/Entities/Job.cs`
- Service: `IJobService` / `JobService` with CRUD methods
- Pages: `Pages/Jobs/Index.cshtml`, `Create.cshtml`, `Details.cshtml`, `Edit.cshtml`
- Navigation properties: `Job.Estimate` (JobEstimate), `Job.Actuals` (JobActuals)
- Job number generation: query max job number for tenant, increment
- Consider `IJobService` methods: `GetAllAsync()`, `GetByIdAsync()`, `CreateAsync()`, `UpdateAsync()`
- Use `[Authorize(Policy = "CanManageJobs")]` for create/edit; all authenticated for read

---

## Dependencies

- Feature 1.2 (EF Core + SQLite)
- Feature 1.3 (Base Entity Model)
- Feature 1.4 (Shared Layout — for consistent page layout)
- Feature 3.1 (Tenant Entity — for FK relationship)

---

## Definition of Done

- [ ] `Job` entity with all fields and relationships configured
- [ ] `IJobService` interface and `JobService` implementation created
- [ ] All four CRUD pages render correctly
- [ ] Search and status filter work on the job list
- [ ] `JobNumber` auto-generates correctly
- [ ] Tenant scoping verified (users only see their own jobs)
- [ ] Role-based access enforced
- [ ] At least 1 unit test for `JobService` logic
- [ ] Manual smoke test passed
