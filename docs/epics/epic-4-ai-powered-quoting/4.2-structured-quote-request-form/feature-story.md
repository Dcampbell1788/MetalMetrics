# Feature 4.2 — Structured Quote Request Form

**Epic:** Epic 4 — AI-Powered Quoting (Claude Integration)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As an** estimator,
**I want** to fill out a structured form describing the fabrication work (material, operations, complexity),
**so that** the AI can generate an accurate cost estimate based on specific job parameters.

---

## Acceptance Criteria

- [ ] AI Quote page accessible at `/Jobs/{jobId}/Quote/AI`
- [ ] Form includes all required input fields (see form fields below)
- [ ] Material type is a dropdown with common sheetmetal options
- [ ] Operations are multi-select checkboxes
- [ ] Complexity is a radio button group (Simple, Moderate, Complex)
- [ ] Form validates required fields before submission
- [ ] Submit button calls the Claude API via `IAIQuoteService`
- [ ] Loading spinner shown while waiting for AI response
- [ ] On success, navigates to AI Quote Review page (Feature 4.4)
- [ ] On API failure, shows error message with option to try again or quote manually
- [ ] Access: Admin, Owner, ProjectManager, Estimator

---

## Form Fields

| Field              | Type       | Options / Notes                                                    |
|--------------------|------------|--------------------------------------------------------------------|
| Material Type      | Dropdown   | Mild Steel, Stainless Steel, Aluminum, Galvanized, Copper, Other   |
| Material Thickness | Text       | Gauge number or decimal inches (e.g., 16ga, 0.060")               |
| Part Dimensions    | Text       | Length x Width (e.g., "24 x 48")                                   |
| Sheet Size Needed  | Text       | Optional — full sheet size if different from part                  |
| Quantity           | Number     | Number of parts                                                    |
| Operations         | Checkboxes | Laser Cut, Brake/Bend, Punch, Weld, Deburr, Roll, Shear, Assembly |
| Complexity         | Radio      | Simple, Moderate, Complex                                          |
| Special Notes      | Textarea   | Free text for additional context                                   |

---

## Technical Notes

- Page: `Pages/Jobs/Quote/AI.cshtml` + `AI.cshtml.cs`
- On submit:
  1. Build `AIQuoteRequest` DTO from form values
  2. Include tenant default rates from `TenantSettings`
  3. Call `IAIQuoteService.GenerateQuoteAsync(request)`
  4. Store response in `TempData` or session for the review page
- Material types could be an enum or a lookup table
- Client-side validation with jQuery Validation (included with .NET template)
- Loading spinner: disable submit button + show spinner overlay during API call

---

## Dependencies

- Feature 4.1 (Claude API Client Service)
- Feature 3.2 (Job Entity — need job context)
- Feature 3.4 (Tenant Settings — for default rates)
- Feature 2.5 (Role-Based Authorization)

---

## Definition of Done

- [ ] AI Quote form renders with all fields
- [ ] Form validation works (client-side and server-side)
- [ ] Form submits to Claude API and receives a response
- [ ] Loading state displayed during API call
- [ ] Error handling works for API failures
- [ ] Access restricted to authorized roles
- [ ] Manual smoke test passed
