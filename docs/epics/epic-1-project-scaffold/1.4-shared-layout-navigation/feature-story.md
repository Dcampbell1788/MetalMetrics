# Feature 1.4 — Shared Layout & Navigation

**Epic:** Epic 1 — Project Scaffold & Infrastructure
**Status:** Complete

---

## User Story

**As a** user,
**I want** a responsive navigation layout with role-aware menu items,
**so that** I can access features relevant to my role from any page.

---

## Implementation

### Layout (`Web/Pages/Shared/_Layout.cshtml`)

- Bootstrap 5 responsive navbar (dark theme)
- "MetalMetrics" branding in navbar
- Role-aware navigation using `User.HasClaim("Role", ...)`:

| Nav Item   | Visible To                            |
|------------|---------------------------------------|
| Dashboard  | Admin, Owner, ProjectManager          |
| Jobs       | All authenticated users               |
| Reports    | Admin, Owner, ProjectManager          |
| Admin      | Admin, Owner                          |

- `@RenderSection("Scripts", required: false)` for per-page scripts

### Partials

- **`_LoginPartial.cshtml`** — User name + Logout (authenticated) or Login/Register links
- **`_Notifications.cshtml`** — TempData alerts (`TempData["Success"]`, `TempData["Error"]`, `TempData["Warning"]`) rendered as Bootstrap alert-success/danger/warning
- **`_ValidationScriptsPartial.cshtml`** — jQuery validation

### Home Page Redirect (`Web/Pages/Index.cshtml.cs`)

Role-based redirect on login:
- Journeyman/Foreman -> `/Jobs?statusFilter=InProgress`
- Estimator -> `/Jobs?statusFilter=Quoted`
- Owner/Admin/PM -> `/Dashboard`

### CSS (`Web/wwwroot/css/site.css`)

```css
:root {
  --mm-profit: #198754;    /* green */
  --mm-loss: #dc3545;      /* red */
  --mm-neutral: #0d6efd;   /* blue */
  --mm-warning: #ffc107;   /* yellow */
  --mm-gold: #ffc107;      /* accent */
}
```

Utility classes: `.text-profit`, `.text-loss`, `.bg-profit`, `.bg-loss`
Card header accent: `.card-header.bg-dark { border-bottom: 2px solid var(--mm-gold); }`
Clickable card hover: `a:hover .card.shadow-sm { transform: translateY(-2px); }`
Print styles: hide nav/footer/buttons, preserve colors with `print-color-adjust: exact`

---

## Definition of Done

- [x] Responsive Bootstrap 5 layout with dark navbar
- [x] Role-aware navigation links
- [x] TempData notification partial
- [x] Login/logout partial
- [x] Color palette CSS variables
- [x] Print-friendly styles
- [x] Home page role-based redirects
