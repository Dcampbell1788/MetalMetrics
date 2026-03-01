# Feature 3.2 — Job Entity & CRUD

**Epic:** Epic 3 — Job & Quote Management
**Status:** Complete

---

## User Story

**As a** project manager or admin,
**I want** to create, view, edit, and manage jobs with status tracking,
**so that** I can organize all fabrication work and track each job through its lifecycle.

---

## Implementation

### Job Entity (`Core/Entities/Job.cs`)

```csharp
public class Job : BaseEntity
{
    public string JobNumber { get; set; } = string.Empty;   // Auto-generated: JOB-0001
    public string Slug { get; set; } = string.Empty;        // 8-char random, URL-friendly
    public string CustomerName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Quoted;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public JobEstimate? Estimate { get; set; }
    public JobActuals? Actuals { get; set; }
    public ICollection<JobAssignment> Assignments { get; set; } = new List<JobAssignment>();
    public ICollection<JobTimeEntry> TimeEntries { get; set; } = new List<JobTimeEntry>();
    public ICollection<JobNote> Notes { get; set; } = new List<JobNote>();
}
```

### JobStatus Enum (`Core/Enums/JobStatus.cs`)

```csharp
public enum JobStatus { Quoted, InProgress, Completed, Invoiced }
```

### IJobService (`Core/Interfaces/IJobService.cs`)

```csharp
Task<List<Job>> GetAllAsync(string? search = null, JobStatus? statusFilter = null);
Task<Job?> GetByIdAsync(Guid id);
Task<Job?> GetByJobNumberAsync(string jobNumber);
Task<Job?> GetBySlugAsync(string slug);
Task<Job> CreateAsync(string customerName, string description);
Task UpdateAsync(Job job);
```

### JobService (`Infrastructure/Services/JobService.cs`)

- `GetAllAsync`: Filters by TenantId, optional search (customer/jobNumber, case-insensitive), optional status filter. Includes Estimate + Actuals. Orders by CreatedAt descending.
- `CreateAsync`: Auto-generates `JobNumber` (queries max for tenant, increments: JOB-0001, JOB-0002, ...) and 8-char random `Slug`
- All queries scoped to current tenant via `ITenantProvider`

### Pages

| Page | Path | Authorization |
|------|------|---------------|
| Job List | `/Jobs/Index` | `[Authorize]` (all authenticated) |
| Create Job | `/Jobs/Create` | `[Authorize(Policy = "CanManageJobs")]` |
| Job Details | `/Jobs/Details/{slug}` | `[Authorize]` |
| Edit Job | `/Jobs/Edit/{slug}` | `[Authorize(Policy = "CanManageJobs")]` |

### Job List Features (`Web/Pages/Jobs/Index.cshtml.cs`)

- Search by customer name or job number
- Filter by status (dropdown)
- Filters persist via session (PRG pattern)
- Query string support: `?statusFilter=Quoted`, `?special=BelowTarget`
- Role-based visibility: Journeyman/Estimator/Foreman only see assigned jobs

### Job Details Features (`Web/Pages/Jobs/Details.cshtml.cs`)

- Status transitions via POST handlers:
  - `OnPostStart`: Quoted -> InProgress
  - `OnPostComplete`: InProgress -> Completed (requires actuals exist, sets CompletedAt)
  - `OnPostInvoice`: Completed -> Invoiced
- Shows estimate, actuals, profitability data
- Links to Quote, Actuals, Profitability, Assignment pages

### Database Indexes

- Unique: `(TenantId, JobNumber)`
- Unique: `(TenantId, Slug)`

---

## Definition of Done

- [x] Job entity with all fields and navigation properties
- [x] JobNumber auto-generation (JOB-XXXX per tenant)
- [x] 8-char Slug for URL-friendly links
- [x] Full CRUD pages (Index, Create, Details, Edit)
- [x] Search and status filter with session persistence
- [x] Query string support for dashboard navigation
- [x] Status transition workflow with validation
- [x] Role-based visibility (assigned jobs only for lower roles)
- [x] 5 unit tests for JobService
