# Feature 2.2 — Tenant Registration (Company Onboarding)

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Complete

---

## User Story

**As a** new user (company owner),
**I want** to register my company and create my admin account in one step,
**so that** I can quickly onboard and start using MetalMetrics.

---

## Implementation

### Page: `/Register` (`Web/Pages/Register.cshtml.cs`)

**Form Fields:** Company Name, Full Name, Email, Password, Confirm Password

**OnPostAsync flow:**
1. Validate form input
2. Create `Tenant` entity with CompanyName
3. Create `TenantSettings` with defaults (labor rate, machine rate, overhead %, target margin %)
4. Create `AppUser` with TenantId, Role = Admin, FullName
5. Use `UserManager.CreateAsync(user, password)` for hashed password
6. Add to "Admin" role via `UserManager.AddToRoleAsync()`
7. All wrapped in try/catch — on failure, rolls back by deleting created tenant
8. Auto sign-in via `SignInManager.SignInAsync()`
9. Redirect to `/Dashboard`

**Validation:**
- Required fields with DataAnnotations
- Email format validation
- Password confirmation match
- Duplicate email rejection (Identity handles this)

### TenantSettings Defaults

Created alongside tenant during registration:
- `DefaultLaborRate` - shop default hourly labor rate
- `DefaultMachineRate` - shop default hourly machine rate
- `DefaultOverheadPercent` - applied to subtotal
- `TargetMarginPercent` - profitability target

---

## Definition of Done

- [x] Registration page renders with all form fields
- [x] Creates Tenant + TenantSettings + AppUser atomically
- [x] User is auto-logged-in and redirected to Dashboard
- [x] Validation errors display correctly
- [x] Duplicate email handled gracefully
