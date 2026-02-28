# Feature 4.4 — AI Quote Review & Acceptance

**Epic:** Epic 4 — AI-Powered Quoting (Claude Integration)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As an** estimator,
**I want** to review the AI-generated quote suggestions, see the reasoning and assumptions, and adjust any values before saving,
**so that** I maintain control over the final estimate while benefiting from AI assistance.

---

## Acceptance Criteria

- [ ] Review page displays AI suggestions alongside editable form fields
- [ ] AI reasoning and assumptions are displayed in a readable format
- [ ] Confidence level is visually indicated (e.g., color-coded badge)
- [ ] All AI-suggested values are editable — the estimator can override any field
- [ ] Real-time recalculation of `TotalEstimatedCost` and `EstimatedMarginPercent` as values change
- [ ] "Accept & Save" button saves the estimate as a `JobEstimate` record
- [ ] `AIGenerated` flag is set to `true` on the saved estimate
- [ ] `AIPromptSnapshot` stores the full prompt and AI response JSON
- [ ] "Start Over" button returns to the AI quote form (Feature 4.2)
- [ ] "Quote Manually" option to go to the manual quote form (Feature 3.3)
- [ ] Access: Admin, Owner, ProjectManager, Estimator

---

## Page Layout

```
┌─────────────────────────────────────────────────────────┐
│  AI Quote Review — Job #JOB-0042                        │
├──────────────────────┬──────────────────────────────────┤
│  AI Suggestions      │  Your Estimate (editable)        │
│                      │                                  │
│  Labor: 4.5 hrs      │  Labor Hours: [4.5    ]          │
│  Material: $285      │  Labor Rate:  [$75.00 ]          │
│  Machine: 2.0 hrs    │  Material:    [$285.00]          │
│  Overhead: 15%       │  Machine Hrs: [2.0    ]          │
│  Suggested: $1,250   │  Machine Rate:[$150.00]          │
│                      │  Overhead %:  [15     ]          │
│  Confidence: Medium  │  Quote Price: [$1,250 ]          │
│                      │                                  │
│                      │  Total Cost:   $987.38           │
│                      │  Margin:       21.0%             │
├──────────────────────┴──────────────────────────────────┤
│  AI Reasoning                                           │
│  "Based on 16ga mild steel, laser cut + 3 bends..."     │
│                                                         │
│  Assumptions                                            │
│  • Standard material pricing at current market rates    │
│  • No special tooling required                          │
│  • Single setup for all operations                      │
├─────────────────────────────────────────────────────────┤
│  [Start Over]    [Quote Manually]    [Accept & Save]    │
└─────────────────────────────────────────────────────────┘
```

---

## Technical Notes

- Page: `Pages/Jobs/Quote/Review.cshtml` + `Review.cshtml.cs`
- AI response passed from Feature 4.2 via `TempData` or session
- Client-side JavaScript recalculates totals and margin as fields change
- On save:
  1. Create `JobEstimate` entity from form values
  2. Set `AIGenerated = true`
  3. Serialize full prompt + response as JSON for `AIPromptSnapshot`
  4. Save to database
  5. Redirect to job details or quote view
- Confidence level display: High = green, Medium = yellow, Low = red

---

## Dependencies

- Feature 4.2 (Structured Quote Request Form — provides AI response data)
- Feature 4.3 (AI Prompt Engineering — response format)
- Feature 3.3 (Quote Entity — for saving the estimate)

---

## Definition of Done

- [ ] Review page displays AI suggestions and editable form
- [ ] Reasoning and assumptions displayed clearly
- [ ] Confidence level visually indicated
- [ ] All fields are editable with real-time cost recalculation
- [ ] Accept & Save creates a `JobEstimate` with `AIGenerated = true`
- [ ] `AIPromptSnapshot` correctly stores the full prompt/response
- [ ] Navigation options (Start Over, Quote Manually) work
- [ ] Role-based access enforced
- [ ] Manual smoke test passed
