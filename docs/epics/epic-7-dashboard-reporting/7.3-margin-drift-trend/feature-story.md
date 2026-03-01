# Feature 7.3 — Margin Drift Trend

**Epic:** Epic 7 — Dashboard & Reporting
**Status:** Complete

---

## User Story

**As a** company owner,
**I want** a line chart showing actual margin over time with my target margin line,
**so that** I can see if quoting accuracy is improving or degrading.

---

## Implementation

### Chart Type

Chart.js v4 line chart with `chartjs-plugin-annotation` for the target line.

### CDN Scripts

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-annotation@3.0.1/dist/chartjs-plugin-annotation.min.js"></script>
```

### Data

Same `JobSummaryDto` data as the bar chart, sorted chronologically by `CompletedAt`.

### Chart Features

- **X-axis:** Job completion dates (formatted via `toLocaleDateString()`)
- **Y-axis:** Actual margin percentage
- **Data points:** Colored green (above target) or red (below target) via `pointBackgroundColor` callback
- **Point radius:** 6px for prominent visibility
- **Line tension:** 0.3 for smooth curves

### Target Margin Annotation Line

Dashed gold horizontal line at `TargetMarginPercent`:
```javascript
annotation: {
    annotations: {
        targetLine: {
            type: 'line',
            yMin: targetMargin,
            yMax: targetMargin,
            borderColor: '#ffc107',
            borderWidth: 2,
            borderDash: [6, 4],
            label: {
                display: true,
                content: 'Target ' + targetMargin + '%',
                position: 'end',
                backgroundColor: 'rgba(255, 193, 7, 0.85)',
                color: '#000'
            }
        }
    }
}
```

### Tooltips

Show job number, customer name, and margin percentage:
```
JOB-0042 — ABC Fabrication
Margin: 18.3%
```

---

## Definition of Done

- [x] Line chart renders with margin data over time
- [x] Dashed gold target margin line with label via annotation plugin
- [x] Data points colored green/red based on above/below target
- [x] Tooltips show job details
- [x] Chart handles varying data points
