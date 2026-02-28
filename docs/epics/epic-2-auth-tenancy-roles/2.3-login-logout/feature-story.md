# Feature 2.3 — Login / Logout

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Small

---

## User Story

**As a** registered user,
**I want** to log in with my email and password and log out when I'm done,
**so that** my session is secure and I can access my company's data.

---

## Acceptance Criteria

- [ ] Login page accessible at `/Login`
- [ ] Login form includes: Email, Password, "Remember Me" checkbox
- [ ] Successful login redirects to `/Dashboard`
- [ ] Failed login shows error message ("Invalid email or password")
- [ ] Logout action accessible at `/Logout` (POST to prevent CSRF)
- [ ] Logout clears the session and redirects to `/Login`
- [ ] Unauthenticated users are automatically redirected to `/Login`
- [ ] "Remember Me" extends cookie lifetime

---

## Technical Notes

- **Pages:** `Pages/Login.cshtml`, `Pages/Logout.cshtml`
- Use `SignInManager<AppUser>.PasswordSignInAsync()` for login
- Use `SignInManager<AppUser>.SignOutAsync()` for logout
- Logout should be a POST action (anti-forgery token included)
- Set `TenantId` and `Role` as claims on login for use by `TenantProvider` and authorization
- Configure `LoginPath` and `AccessDeniedPath` in cookie auth options

---

## Dependencies

- Feature 2.1 (ASP.NET Identity Setup)
- Feature 2.2 (Tenant Registration — need at least one user to log in)

---

## Definition of Done

- [ ] Login page renders with email/password form
- [ ] Valid credentials log the user in and redirect to Dashboard
- [ ] Invalid credentials show an error message
- [ ] Logout clears session and redirects to Login
- [ ] Unauthenticated access redirects to Login
- [ ] Manual smoke test passed
