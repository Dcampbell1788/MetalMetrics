# Feature 4.3 — AI Prompt Engineering

**Epic:** Epic 4 — AI-Powered Quoting
**Status:** Complete

---

## User Story

**As a** developer,
**I want** carefully engineered prompts that leverage Claude's domain knowledge,
**so that** the AI generates realistic, structured cost estimates.

---

## Implementation

### PromptTemplates (`Infrastructure/Services/PromptTemplates.cs`)

**System Prompt (constant):**
```
You are an expert sheetmetal fabrication estimator with 20+ years of experience...
You MUST respond with ONLY a JSON object (no markdown, no explanation outside JSON).
```

**Required JSON schema:**
```json
{
  "estimatedLaborHours": <number>,
  "estimatedMaterialCost": <number>,
  "estimatedMachineHours": <number>,
  "overheadPercent": <number>,
  "suggestedQuotePrice": <number>,
  "reasoning": "<string>",
  "assumptions": ["<string>"],
  "confidenceLevel": "Low" | "Medium" | "High"
}
```

**Rules enforced in prompt:**
- All numeric values must be positive
- Use the provided shop rates
- Add 15-25% profit margin to suggested quote price
- Consider material waste, setup time, and complexity

**BuildUserPrompt method:**
Formats job details into structured text:
```
Estimate the following sheetmetal job:
- Material: {type}, {thickness}
- Part Dimensions: {dimensions}
- Sheet Size: {sheetSize}
- Quantity: {qty}
- Operations: {ops}
- Complexity: {level}
- Notes: {notes}

Shop rates:
- Labor: ${rate}/hr
- Machine: ${rate}/hr
- Overhead: {pct}%
```

### AIQuoteResponse DTO (`Core/DTOs/AIQuoteResponse.cs`)

```csharp
public class AIQuoteResponse
{
    public decimal EstimatedLaborHours { get; set; }
    public decimal EstimatedMaterialCost { get; set; }
    public decimal EstimatedMachineHours { get; set; }
    public decimal OverheadPercent { get; set; }
    public decimal SuggestedQuotePrice { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Assumptions { get; set; } = new();
    public string ConfidenceLevel { get; set; } = "Medium";
}
```

### Response Parsing

- `System.Text.Json` with `JsonSerializerOptions { PropertyNameCaseInsensitive = true }`
- Strips markdown code fences before parsing
- Returns null + error if JSON is invalid

---

## Definition of Done

- [x] System prompt establishes sheetmetal domain expertise
- [x] Structured JSON response schema enforced
- [x] BuildUserPrompt formats job details + shop rates
- [x] AIQuoteResponse DTO with all fields
- [x] Prompt snapshot stored for audit
- [x] Prompts stored in PromptTemplates class (not inline)
