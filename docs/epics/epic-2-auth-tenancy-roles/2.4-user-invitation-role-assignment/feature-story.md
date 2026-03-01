# Feature 2.4 — User Invitation & Role Assignment

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Complete

---

## User Story

**As an** Admin or Owner,
**I want** to invite team members by email and assign them a role,
**so that** my team can access MetalMetrics with appropriate permissions.

---

## Implementation

### Page: `/Admin/Users` (`Web/Pages/Admin/Users.cshtml.cs`)

**Authorization:** `[Authorize(Policy = "AdminOnly")]` (Admin + Owner only)

**User List:**
- Displays all users in the current tenant
- Shows: Full Name, Email, Role, Lockout status
- Tenant-scoped: queries `UserManager.Users.Where(u => u.TenantId == tenantId)`

**Invite User (OnPostInvite):**
1. Form: Email, Full Name, Role (dropdown of all AppRole values)
2. Creates `AppUser` with `UserManager.CreateAsync(user, tempPassword)`
3. Adds to Identity role via `UserManager.AddToRoleAsync()`
4. Displays generated temp password to admin on screen
5. Invited user logs in with temp password

**Update Role (OnPostUpdateRole):**
- Changes user's `Role` property
- Removes old Identity role, adds new one

**Lockout/Unlock (OnPostToggleLockout):**
- `UserManager.SetLockoutEndDateAsync()` with future date to lock
- `null` lockout end date to unlock

### Temp Password Generation

Simple approach: displayed on-screen after invite (no email flow). Admin gives password to user directly.

---

## Definition of Done

- [x] `/Admin/Users` renders user list for current tenant
- [x] Invite form creates new user with selected role
- [x] Temp password displayed after invite
- [x] Role can be changed from admin page
- [x] User lockout/unlock working
- [x] Page restricted to Admin/Owner roles
