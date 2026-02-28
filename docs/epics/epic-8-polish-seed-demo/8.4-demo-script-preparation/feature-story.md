# Feature 8.4 — Demo Script Preparation

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Pending
**Priority:** High
**Estimated Effort:** Small

---

## User Story

**As a** demo presenter,
**I want** a rehearsed demo script with a clear narrative arc,
**so that** I can deliver a compelling 5-minute walkthrough that answers "Did we make money?" and showcases the AI quoting feature.

---

## Acceptance Criteria

- [ ] Written demo script with step-by-step walkthrough
- [ ] Demo covers the complete happy path (register → quote → actuals → profitability → dashboard)
- [ ] At least one "money lost" job highlighted for dramatic storytelling
- [ ] AI quoting feature showcased with live Claude API call
- [ ] Dashboard overview demonstrates business insights at a glance
- [ ] Demo credentials documented (email/password for demo accounts)
- [ ] Fallback plan if Claude API is unavailable during demo
- [ ] Estimated timing: fits within 5 minutes

---

## Demo Script Outline

### Act 1: Setup (30 seconds)
1. Open application → show login page
2. "MetalMetrics helps sheetmetal shops answer one question: Did we make money?"
3. Log in as "Precision Metal Works" owner

### Act 2: Create & Quote a Job (90 seconds)
4. Navigate to Jobs → "Let's create a new job"
5. Create job: "Metro Industries - Stainless bracket order"
6. Click "AI Quote" → fill out structured form
7. Submit → show Claude thinking (loading spinner)
8. Review AI suggestions, reasoning, and confidence level
9. "Our estimator can adjust any values" → Accept & Save

### Act 3: The Reveal — Did We Make Money? (90 seconds)
10. Navigate to a pre-existing completed job (from seed data)
11. "This job was completed last week. Let's see how we did."
12. Open profitability view → dramatic reveal: PROFIT or LOSS
13. Walk through the variance breakdown by category
14. "We underestimated labor by 15% — that's a pattern we need to fix"

### Act 4: The Big Picture (60 seconds)
15. Navigate to Dashboard
16. KPI cards: "42 jobs, 18% average margin, 7 over budget"
17. Show profitability chart: "These red bars are our problem jobs"
18. Customer breakdown: "Smith & Sons is actually costing us money"
19. Category heatmap: "We consistently underestimate labor"

### Act 5: Close (30 seconds)
20. "MetalMetrics gives shop owners clarity. No more guessing."
21. Quick mention: multi-tenant, role-based, mobile-friendly
22. "Questions?"

---

## Technical Notes

- Pre-stage the browser with the right tab open to save time
- Have seed data loaded and verified before the demo
- Demo user accounts:
  - Owner: `owner@precisionmetal.demo` / `Demo123!`
  - Owner: `owner@budgetfab.demo` / `Demo123!`
- Ensure Claude API key is active and working
- Fallback if API is down: show a pre-saved AI response, explain manual quote flow
- Test the full demo flow at least twice before presenting
- Consider screen recording as a backup

---

## Dependencies

- Feature 8.1 (Seed Data — must be loaded)
- Feature 8.2 (UX Polish — app must look professional)
- Feature 8.3 (Error Handling — no crashes during demo)
- All Epics 1–7 features functional

---

## Definition of Done

- [ ] Demo script written and reviewed by team
- [ ] Full walkthrough completed successfully at least twice
- [ ] Demo accounts and seed data verified
- [ ] Timing fits within 5 minutes
- [ ] Fallback plan documented for API failure
- [ ] Team member assigned as presenter
