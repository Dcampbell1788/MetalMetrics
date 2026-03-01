# Feature 2.3 — Login / Logout

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Complete

---

## User Story

**As a** registered user,
**I want** to log in with my email and password and log out when done,
**so that** my session is secure and I can access my company's data.

---

## Implementation

### Login Page (`Web/Pages/Login.cshtml.cs`)

**Form Fields:** Email, Password, Remember Me checkbox

**OnPostAsync flow:**
1. Find user by email via `UserManager.FindByEmailAsync()`
2. Check lockout status
3. Attempt sign-in via `SignInManager.PasswordSignInAsync()`
4. On success: add/update custom claims (`TenantId`, `FullName`) via `UserManager.AddClaimAsync()`
5. Sign in with updated claims
6. Redirect to `/` (which redirects by role to Dashboard or Jobs)

**Claim Setup on Login:**
- `TenantId` claim — used by `TenantProvider` for all tenant-scoped queries
- `FullName` claim — displayed in UI

### Logout

Handled via `_LoginPartial.cshtml` with a form POST to `/Login?handler=Logout`:
- Calls `SignInManager.SignOutAsync()`
- Redirects to `/Login`

### Cookie Configuration

```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
});
```

---

## Definition of Done

- [x] Login page with email/password/remember-me
- [x] Custom claims (TenantId, FullName) set on login
- [x] Logout clears session and redirects to Login
- [x] Unauthenticated access redirects to Login
- [x] Lockout status check before sign-in
