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
    options.AddPolicy("AdminOnly", p =>
        p.RequireClaim("Role", "Admin", "Owner"));
    options.AddPolicy("CanManageJobs", p =>
        p.RequireClaim("Role", "Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanQuote", p =>
        p.RequireClaim("Role", "Admin", "Owner", "ProjectManager", "Estimator"));
    options.AddPolicy("CanEnterActuals", p =>
        p.RequireClaim("Role", "Admin", "Owner", "ProjectManager", "Foreman"));
    options.AddPolicy("CanViewReports", p =>
        p.RequireClaim("Role", "Admin", "Owner", "ProjectManager"));
    options.AddPolicy("CanAssignJobs", p =>
        p.RequireClaim("Role", "Admin", "Owner", "ProjectManager", "Foreman"));
});
```

Note: Uses `RequireClaim("Role", ...)` not `RequireRole(...)` because roles are stored as custom claims.

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

`_Layout.cshtml` uses `User.HasClaim("Role", ...)` to show/hide nav items per role.

### TenantProvider

Reads `TenantId` claim from `HttpContext.User.Claims`. Returns `Guid.Empty` if unauthenticated.

---

## Definition of Done

- [x] 6 authorization policies registered
- [x] All pages enforce correct role restrictions
- [x] Nav menu is role-aware
- [x] TenantProvider reads TenantId from claims
- [x] Role-based job visibility for Journeyman/Estimator/Foreman
