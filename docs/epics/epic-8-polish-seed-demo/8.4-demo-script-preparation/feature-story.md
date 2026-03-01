# Feature 8.4 — Demo Script Preparation

**Epic:** Epic 8 — Polish, Seed Data & Demo Prep
**Status:** Complete

---

## User Story

**As a** demo presenter,
**I want** a rehearsed demo script with a clear narrative,
**so that** I can deliver a compelling walkthrough answering "Did we make money?"

---

## Implementation

### Demo Credentials

All passwords: `Demo123!`

**Precision Metal Works** (profitable):
- Owner: `mike@precisionmetal.demo`
- Admin: `karen@precisionmetal.demo`
- PM: `dave@precisionmetal.demo`

**Budget Fabricators** (struggling):
- Owner: `daryl@budgetfab.demo`
- PM: `greg@budgetfab.demo`

### Demo Script (5 minutes)

**Act 1 — Setup (30s):**
1. Show login page: "MetalMetrics helps sheetmetal shops answer: Did we make money?"
2. Login as `mike@precisionmetal.demo`

**Act 2 — Create & Quote (90s):**
3. Jobs -> Create new job (customer name, description)
4. Click "AI Quote" -> fill structured form (material, ops, complexity)
5. Show Claude thinking (loading spinner)
6. Review AI suggestions, reasoning, confidence
7. Adjust values -> Accept & Save

**Act 3 — Profitability Reveal (90s):**
8. Navigate to a completed job from seed data
9. Open Profitability view -> PROFIT or LOSS reveal
10. Walk through category variances
11. "We underestimated labor by X% — that's a pattern"

**Act 4 — Dashboard (60s):**
12. Dashboard KPIs: total jobs, avg margin, below target count
13. At-risk alerts (if any InProgress jobs are over budget)
14. Work pipeline: Quoted/InProgress counts
15. Charts: Est vs Actual bars, margin trend with target line
16. Customer table: "Which customers make us money?"
17. Estimating accuracy: "We consistently underestimate labor"

**Act 5 — Contrast (30s):**
18. Login as `daryl@budgetfab.demo` -> worse numbers, more red
19. "Two companies, same tool, different stories"
20. "Questions?"

### Fallback Plan

If Claude API is unavailable:
- Show manual quote form as fallback
- Explain AI feature verbally with pre-saved example
- Focus demo on profitability/dashboard features (all work offline)

### Pre-Demo Checklist

1. Delete `metalmetrics.db`, rebuild, run app (ensures clean seed data)
2. Verify both logins work
3. Test Claude API with a sample quote
4. Pre-stage browser tabs

---

## Definition of Done

- [x] Demo script written with timing
- [x] Demo credentials documented
- [x] Fallback plan for API failure
- [x] Seed data produces compelling dashboard
- [x] Both tenant stories contrast effectively
