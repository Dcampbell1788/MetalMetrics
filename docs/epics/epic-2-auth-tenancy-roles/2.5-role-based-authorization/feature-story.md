# Feature 2.5 — Role-Based Authorization

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Medium

---

## User Story

**As a** system administrator,
**I want** page-level and feature-level access control based on user roles,
**so that** users can only access the features appropriate to their position in the company.

---

## Acceptance Criteria

- [ ] Authorization policies defined for each role group
- [ ] Page-level `[Authorize]` attributes enforce role restrictions
- [ ] Navigation menu hides links the user doesn't have access to
- [ ] `TenantProvider` reads `TenantId` from the authenticated user's claims
- [ ] Unauthorized access returns a 403 / Access Denied page
- [ ] Custom `AccessDenied` page with a user-friendly message
- [ ] Role hierarchy enforced per the role matrix below

---

## Role Access Matrix

| Feature Area     | Admin | Owner | ProjectManager | Foreman | Estimator | Journeyman |
|------------------|-------|-------|----------------|---------|-----------|------------|
| Jobs (full)      | Yes   | Yes   | Yes            | Yes     | Read-only | Read-only  |
| Quotes           | Yes   | Yes   | Yes            | No      | Yes       | No         |
| Actuals          | Yes   | Yes   | Yes            | Yes     | No        | Yes        |
| Reports          | Yes   | Yes   | Yes            | Yes     | No        | No         |
| Admin/Billing    | Yes   | Yes   | No             | No      | No        | No         |

---

## Technical Notes

- Define policies in `Program.cs`:
  ```csharp
  builder.Services.AddAuthorization(options =>
  {
      options.AddPolicy("AdminOnly", p => p.RequireRole("Admin", "Owner"));
      options.AddPolicy("CanManageJobs", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
      options.AddPolicy("CanQuote", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Estimator"));
      options.AddPolicy("CanEnterActuals", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman", "Journeyman"));
      options.AddPolicy("CanViewReports", p => p.RequireRole("Admin", "Owner", "ProjectManager", "Foreman"));
  });
  ```
- Apply `[Authorize(Policy = "...")]` on Razor Page models
- `TenantProvider` implementation reads `TenantId` claim set during login (Feature 2.3)
- Ensure global query filter uses `TenantProvider.TenantId` so users never see other tenants' data

---

## Dependencies

- Feature 2.1 (ASP.NET Identity Setup)
- Feature 2.3 (Login — claims must be set on login)

---

## Definition of Done

- [ ] Authorization policies registered in DI
- [ ] All pages enforce correct role restrictions
- [ ] Nav menu is role-aware (hides unauthorized links)
- [ ] `TenantProvider` correctly reads tenant from claims
- [ ] Access Denied page renders for unauthorized access
- [ ] At least 1 unit test for authorization policy logic
- [ ] Manual smoke test: verify each role sees correct nav items
