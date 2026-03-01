# Feature 2.5 — Role-Based Authorization

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Complete

---

## User Story

**As a** system administrator,
**I want** page-level access control based on user roles,
**so that** users only access features appropriate to their position.

---

## Implementation

### Authorization Policies (`Web/Program.cs`)

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin", "Owner"));
    options.AddPolicy("CanManageJobs", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanQuote", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Estimator"));
    options.AddPolicy("CanEnterActuals", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanViewReports", p => p.RequireRole("Admin", "Owner", "ProjectManager"));
    options.AddPolicy("CanAssignJobs", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
});
```

Uses `RequireRole(...)` with ASP.NET Identity roles (users are added to roles via `UserManager.AddToRoleAsync()`).

### Page-Level Authorization

| Page Area | Policy | Attribute |
|-----------|--------|-----------|
| Dashboard | CanViewReports | `[Authorize(Policy = "CanViewReports")]` |
| Jobs Index | (any authenticated) | `[Authorize]` |
| Jobs Create/Edit | CanManageJobs | `[Authorize(Policy = "CanManageJobs")]` |
| Quote pages | CanQuote | `[Authorize(Policy = "CanQuote")]` |
| Actuals pages | CanEnterActuals | `[Authorize(Policy = "CanEnterActuals")]` |
| Profitability | CanViewReports | `[Authorize(Policy = "CanViewReports")]` |
| Reports | CanViewReports | `[Authorize(Policy = "CanViewReports")]` |
| Admin | AdminOnly | `[Authorize(Policy = "AdminOnly")]` |
| Job Assignment | CanAssignJobs | `[Authorize(Policy = "CanAssignJobs")]` |

### Role-Based Job Visibility

Journeyman, Estimator, and Foreman only see jobs they are assigned to (filtered in `Jobs/Index.cshtml.cs` via `IJobAssignmentService`).

### Navigation Menu

`_Layout.cshtml` uses `User.IsInRole(...)` to show/hide nav items per role.

### AccessDenied Page (`Web/Pages/AccessDenied.cshtml.cs`)

When a user tries to access a page they don't have permission for, they are redirected to `/AccessDenied` (configured via `options.AccessDeniedPath = "/AccessDenied"` in Program.cs). Shows a friendly message instead of a raw 403.

### TenantProvider

Reads `TenantId` claim from `HttpContext.User.Claims`. Returns `Guid.Empty` if unauthenticated.

---

## Definition of Done

- [x] 6 authorization policies registered
- [x] All pages enforce correct role restrictions
- [x] Nav menu is role-aware (uses `User.IsInRole()`)
- [x] AccessDenied page for unauthorized access attempts
- [x] TenantProvider reads TenantId from claims
- [x] Role-based job visibility for Journeyman/Estimator/Foreman
