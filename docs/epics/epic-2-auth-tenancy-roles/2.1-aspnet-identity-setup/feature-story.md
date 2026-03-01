# Feature 2.1 — ASP.NET Identity Setup

**Epic:** Epic 2 — Authentication, Tenancy & Role Management
**Status:** Complete

---

## User Story

**As a** developer,
**I want** ASP.NET Identity configured with a custom `AppUser` that includes tenant and role information,
**so that** user authentication and tenant association are handled by the framework.

---

## Implementation

### AppUser Entity (`Core/Entities/AppUser.cs`)

```csharp
public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public AppRole Role { get; set; }
    public Tenant? Tenant { get; set; }
}
```

Note: `AppUser` extends `IdentityUser` (not `BaseEntity`). It has its own `TenantId`.

### AppRole Enum (`Core/Enums/AppRole.cs`)

```csharp
public enum AppRole
{
    Admin, Owner, ProjectManager, Foreman, Estimator, Journeyman
}
```

### Identity Registration (`Web/Program.cs`)

```csharp
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/Login";
});
```

### Role Seeding (Program.cs startup)

All 6 roles are created on startup via `RoleManager<IdentityRole>`:
```csharp
var roles = new[] { "Admin", "Owner", "ProjectManager", "Foreman", "Estimator", "Journeyman" };
foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
}
```

---

## Definition of Done

- [x] AppUser entity with FullName, TenantId, Role (AppRole enum)
- [x] Identity configured with SQLite store
- [x] Cookie auth with redirect to /Login
- [x] Password policy: digit + lowercase required, 6+ chars
- [x] All 6 roles seeded on startup
