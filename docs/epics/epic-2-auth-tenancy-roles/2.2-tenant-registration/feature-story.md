# Feature 2.2 — Tenant Registration (Company Onboarding)

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Pending
**Priority:** Critical
**Estimated Effort:** Medium

---

## User Story

**As a** new user (company owner),
**I want** to register my company and create my admin account in one step,
**so that** I can quickly onboard and start using MetalMetrics without a complicated setup process.

---

## Acceptance Criteria

- [ ] Registration page accessible at `/Register`
- [ ] Registration form includes: Company Name, Owner Full Name, Email, Password, Confirm Password
- [ ] On submit, creates a new `Tenant` entity with the company name
- [ ] Creates the first `AppUser` linked to that tenant with the `Admin` role
- [ ] User is automatically logged in after successful registration
- [ ] Redirects to Dashboard after registration
- [ ] Validates: required fields, email format, password strength, passwords match
- [ ] Displays validation errors inline on the form
- [ ] Duplicate email addresses are rejected with a clear message

---

## Technical Notes

- **Page:** `Pages/Register.cshtml` + `Register.cshtml.cs`
- Flow:
  1. Validate form input
  2. Create `Tenant` record
  3. Create `AppUser` with `TenantId`, `Role = Admin`, `FullName`
  4. Sign in the user via `SignInManager`
  5. Redirect to `/Dashboard`
- Use `UserManager<AppUser>.CreateAsync()` for user creation
- Wrap tenant + user creation in a transaction to ensure atomicity

---

## Dependencies

- Feature 2.1 (ASP.NET Identity Setup)
- Feature 3.1 (Tenant Entity — must exist for creating tenant records)

---

## Definition of Done

- [ ] Registration page renders with all form fields
- [ ] Successful registration creates Tenant + AppUser in the database
- [ ] User is auto-logged-in and redirected to Dashboard
- [ ] Validation errors display correctly
- [ ] Duplicate email is handled gracefully
- [ ] Manual smoke test passed
