# Feature 4.3 — AI Prompt Engineering

**Epic:** Epic 4 — AI-Powered Quoting (Claude Integration)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** developer,
**I want** carefully engineered prompts that leverage Claude's domain knowledge of sheetmetal fabrication,
**so that** the AI generates realistic, structured cost estimates that estimators can trust and refine.

---

## Acceptance Criteria

- [ ] System prompt establishes Claude as a sheetmetal fabrication estimating expert
- [ ] Prompt includes the tenant's default rates as context
- [ ] Prompt requests a structured JSON response with specific fields
- [ ] Response includes: estimated hours, costs, suggested quote price, reasoning, assumptions, confidence level
- [ ] Response JSON schema is well-defined and consistently parseable
- [ ] Prompt handles edge cases: unusual materials, very large/small quantities, complex assemblies
- [ ] Prompt versioning: system prompt is stored as a constant or config for easy iteration
- [ ] AI response is validated against the expected schema before use

---

## Prompt Structure

### System Prompt
```
You are a sheetmetal fabrication estimating expert with 20+ years of experience.
You provide cost estimates for custom metal fabrication jobs including laser cutting,
brake forming, welding, and finishing operations.

Provide your estimates in the following JSON format:
{
  "estimatedLaborHours": <number>,
  "estimatedMaterialCost": <number>,
  "estimatedMachineHours": <number>,
  "overheadPercent": <number>,
  "suggestedQuotePrice": <number>,
  "reasoning": "<string>",
  "assumptions": ["<string>", ...],
  "confidenceLevel": "Low" | "Medium" | "High"
}
```

### User Prompt (per request)
```
Estimate the following sheetmetal job:
- Material: {type}, {thickness}
- Dimensions: {L x W}
- Quantity: {qty}
- Operations: {comma-separated list}
- Complexity: {level}
- Notes: {notes}

Use these shop rates:
- Labor: ${laborRate}/hr
- Machine: ${machineRate}/hr
- Overhead: {overheadPercent}%
```

---

## Technical Notes

- Store prompts as constants in a `PromptTemplates` class or in configuration
- Use string interpolation or a template engine to build the user prompt
- JSON parsing: use `System.Text.Json` to deserialize the AI response
- Validate response: check all required fields are present and numeric values are positive
- If response doesn't parse, return a structured error and let the user retry or quote manually
- Consider logging the full prompt + response for debugging and quality improvement
- The `AIPromptSnapshot` field on `JobEstimate` stores the complete prompt + response

---

## Response DTO

```csharp
public class AIQuoteResponse
{
    public decimal EstimatedLaborHours { get; set; }
    public decimal EstimatedMaterialCost { get; set; }
    public decimal EstimatedMachineHours { get; set; }
    public decimal OverheadPercent { get; set; }
    public decimal SuggestedQuotePrice { get; set; }
    public string Reasoning { get; set; }
    public List<string> Assumptions { get; set; }
    public string ConfidenceLevel { get; set; } // "Low", "Medium", "High"
}
```

---

## Dependencies

- Feature 4.1 (Claude API Client Service)
- Feature 3.4 (Tenant Settings — for shop rates in prompt)

---

## Definition of Done

- [ ] System prompt and user prompt templates defined
- [ ] Prompts produce consistent, parseable JSON responses from Claude
- [ ] Response DTO created and JSON deserialization works
- [ ] Edge cases handled (unparseable response, missing fields)
- [ ] Prompt snapshot saved for auditing
- [ ] At least 3 test prompts run with reasonable results
- [ ] Prompt templates stored in a maintainable location (not inline strings)
