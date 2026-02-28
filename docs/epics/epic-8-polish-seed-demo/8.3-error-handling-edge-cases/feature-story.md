# Feature 8.3 — Error Handling & Edge Cases

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Pending
**Priority:** Medium
**Estimated Effort:** Medium

---

## User Story

**As a** user,
**I want** the application to handle errors gracefully without crashing or showing technical details,
**so that** I always know what happened and what to do next, even when something goes wrong.

---

## Acceptance Criteria

- [ ] Claude API timeout shows a user-friendly error with "Try Again" and "Quote Manually" options
- [ ] Claude API failure (rate limit, server error) handled with fallback messaging
- [ ] Empty states display helpful messages with call-to-action:
  - No jobs yet → "Create your first job"
  - No actuals entered → "Enter actuals for this job"
  - No completed jobs → "Complete a job to see reports"
- [ ] Duplicate actuals entry prevented (only one `JobActuals` per job)
- [ ] Division-by-zero in margin calculations returns 0% or N/A, not an exception
- [ ] Global error handler catches unhandled exceptions and shows a friendly error page
- [ ] 404 page styled to match the application
- [ ] Form inputs validated for type, range, and required fields
- [ ] Negative numbers handled appropriately in cost fields

---

## Error Scenarios

| Scenario                        | Expected Behavior                                    |
|---------------------------------|------------------------------------------------------|
| Claude API timeout              | "AI is taking too long. Try again or quote manually" |
| Claude API rate limited         | "AI service is busy. Please wait and try again."     |
| Claude API returns bad JSON     | "Couldn't parse AI response. Try again."             |
| Network failure                 | "Unable to connect. Check your connection."          |
| No estimate exists for job      | "Create an estimate before entering actuals."        |
| Duplicate actuals submit        | Load existing actuals for editing instead            |
| Zero cost in margin calc        | Display "N/A" instead of division error              |
| Invalid form input              | Inline validation error message                      |
| Unauthorized page access        | Redirect to Access Denied page                       |
| Job not found (bad URL/ID)      | 404 page with link back to job list                  |

---

## Technical Notes

- Global error handler: `app.UseExceptionHandler("/Error")` with custom error page
- Custom 404: `app.UseStatusCodePagesWithReExecute("/Error/{0}")`
- AI error handling: wrap API calls in try-catch in `ClaudeAIQuoteService`
- Division-by-zero guards:
  ```csharp
  public static decimal SafeMargin(decimal revenue, decimal cost)
      => revenue == 0 ? 0 : (revenue - cost) / revenue * 100;
  ```
- Empty state partials: create reusable `_EmptyState.cshtml` partial
- Duplicate prevention: check for existing `JobActuals` before insert, update if exists
- Input validation: use Data Annotations and model-level validation
- Log errors for debugging but never expose stack traces to users

---

## Dependencies

- All feature pages (Epics 1–7) should be functional
- Feature 4.1 (Claude API Client — for AI error handling)

---

## Definition of Done

- [ ] AI failures handled gracefully with user-friendly messages
- [ ] All empty states display helpful messages
- [ ] Division-by-zero prevented in all calculations
- [ ] Duplicate data entry prevented
- [ ] Global error page works for unhandled exceptions
- [ ] 404 page styled and functional
- [ ] Form validation covers all edge cases
- [ ] Manual testing of each error scenario
