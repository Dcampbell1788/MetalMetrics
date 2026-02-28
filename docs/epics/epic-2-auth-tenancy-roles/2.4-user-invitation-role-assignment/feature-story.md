# Feature 2.4 — User Invitation & Role Assignment

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As an** Admin or Owner,
**I want** to invite team members by email and assign them a role,
**so that** my team can access MetalMetrics with appropriate permissions for their job function.

---

## Acceptance Criteria

- [ ] Admin page accessible at `/Admin/Users` (Admin and Owner roles only)
- [ ] Page displays a list of all users in the current tenant
- [ ] "Invite User" form includes: Email, Full Name, Role (dropdown)
- [ ] Available roles: `Admin`, `Owner`, `ProjectManager`, `Foreman`, `Estimator`, `Journeyman`
- [ ] Inviting creates a pre-provisioned `AppUser` record with a temporary password
- [ ] Invited user can log in with the temporary password
- [ ] Admin can change a user's role from the user list
- [ ] Admin can deactivate a user (soft delete or lock out)
- [ ] Only users within the same tenant are visible

---

## Technical Notes

- **Page:** `Pages/Admin/Users.cshtml` + `Users.cshtml.cs`
- Simple invite flow (hackathon scope):
  1. Admin enters email, name, role
  2. System creates `AppUser` with generated temp password
  3. Display temp password on screen (or copy to clipboard)
  4. Invited user logs in and (optionally) changes password
- For hackathon, skip email-based invite links — just show the temp password
- Use `UserManager<AppUser>.CreateAsync(user, tempPassword)` to create
- Tenant scoping: only show users where `user.TenantId == currentTenantId`

---

## Dependencies

- Feature 2.1 (ASP.NET Identity Setup)
- Feature 2.5 (Role-Based Authorization — to restrict this page to Admin/Owner)

---

## Definition of Done

- [ ] `/Admin/Users` page renders user list for current tenant
- [ ] Invite form creates a new user with the selected role
- [ ] Temp password is displayed after invite
- [ ] Invited user can log in successfully
- [ ] Role can be changed from the admin page
- [ ] Page is restricted to Admin/Owner roles
- [ ] Manual smoke test passed
