# Feature 8.2 — UX Polish

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Medium

---

## User Story

**As a** user of MetalMetrics,
**I want** a polished, professional-looking interface with clear feedback and responsive design,
**so that** the application feels trustworthy and is easy to use on any device.

---

## Acceptance Criteria

- [ ] Loading spinners displayed on AI quote requests and any slow operations
- [ ] Form validation messages shown for both client-side and server-side errors
- [ ] Responsive design works on desktop, tablet, and mobile viewports
- [ ] Consistent color palette applied throughout:
  - Green = profit / positive / success
  - Red = loss / negative / error
  - Blue = neutral / informational
- [ ] Print-friendly CSS for the profitability report view
- [ ] All interactive elements have hover states and focus indicators
- [ ] Toast notifications for success/error feedback on actions
- [ ] No broken layouts, overlapping elements, or horizontal scroll issues
- [ ] Favicon and page titles set appropriately

---

## Polish Checklist

### Visual Consistency
- [ ] Consistent spacing and margins across all pages
- [ ] Consistent typography (headings, body text, labels)
- [ ] Consistent button styles (primary, secondary, danger)
- [ ] Consistent table styles across job list, user list, reports

### Feedback & Loading
- [ ] Loading spinner on AI quote submission
- [ ] Disabled submit buttons during form submission (prevent double-submit)
- [ ] Success toast after saving a job, quote, or actuals
- [ ] Error toast for failed operations
- [ ] Confirmation dialogs for destructive actions (delete, status change)

### Responsive Design
- [ ] Nav collapses to hamburger menu on mobile
- [ ] Tables scroll horizontally on mobile (not break layout)
- [ ] Forms stack vertically on mobile
- [ ] Charts resize appropriately
- [ ] Quick Entry form (Feature 5.3) tested on mobile viewport

### Print
- [ ] Print stylesheet hides navigation and non-essential UI
- [ ] Profitability report prints cleanly on a single page
- [ ] Charts print with visible colors (no transparency issues)

---

## Technical Notes

- Use Bootstrap 5 utility classes for most responsive work
- Loading spinner: Bootstrap spinner component or custom CSS animation
- Toast notifications: Bootstrap toast component or a lightweight library
- Print CSS: `@media print { ... }` stylesheet
- Consider a `site.css` override file for custom brand colors
- Color variables:
  ```css
  :root {
    --mm-profit: #28a745;
    --mm-loss: #dc3545;
    --mm-neutral: #007bff;
    --mm-warning: #ffc107;
  }
  ```

---

## Dependencies

- All feature pages must be built first (Epics 1–7)

---

## Definition of Done

- [ ] All pages visually consistent
- [ ] Loading states work on AI and slow operations
- [ ] Form validation messages display correctly
- [ ] Responsive design verified on desktop, tablet, and mobile
- [ ] Print profitability report works
- [ ] Color palette consistent throughout
- [ ] Manual visual review of all pages
