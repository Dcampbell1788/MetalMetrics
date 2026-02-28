# Feature 1.4 — Shared Layout & Navigation

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** user of MetalMetrics,
**I want** a clean, responsive navigation layout with role-aware menu items,
**so that** I can easily navigate the application and only see features relevant to my role.

---

## Acceptance Criteria

- [ ] `_Layout.cshtml` created with responsive navigation bar
- [ ] Navigation includes links to: Dashboard, Jobs, Admin (role-dependent)
- [ ] Menu items are conditionally shown/hidden based on user role
- [ ] "MetalMetrics" branding (logo or text) displayed in the nav bar
- [ ] Toast/notification partial created for success and error messages
- [ ] Bootstrap 5 CSS framework integrated (ships with .NET template)
- [ ] Layout is responsive and works on desktop, tablet, and mobile
- [ ] Footer with minimal branding

---

## Technical Notes

- Bootstrap 5 is included by default in the .NET 8 Razor Pages template
- Use `User.IsInRole("Admin")` or claim checks for conditional nav items
- Role-based nav visibility:

  | Nav Item   | Visible To                              |
  |------------|-----------------------------------------|
  | Dashboard  | Admin, Owner, ProjectManager, Foreman   |
  | Jobs       | All authenticated users                 |
  | Quotes     | Admin, Owner, ProjectManager, Estimator |
  | Actuals    | Admin, Owner, PM, Foreman, Journeyman   |
  | Reports    | Admin, Owner, ProjectManager            |
  | Admin      | Admin, Owner                            |

- Toast notifications: use a `TempData`-based approach or a `_Notifications.cshtml` partial
- Consider a `_LoginPartial.cshtml` for login/logout/user display in the nav

---

## Dependencies

- Feature 1.1 (Solution & Project Structure)
- Feature 2.5 (Role-Based Authorization — for role checks in nav, but can stub initially)

---

## Definition of Done

- [ ] `_Layout.cshtml` renders a responsive nav bar with branding
- [ ] Nav items are role-aware (hide/show based on role)
- [ ] Toast/notification partial works for success and error messages
- [ ] Layout is mobile-responsive
- [ ] Manual visual smoke test passed on desktop and mobile viewport
