# Feature 4.1 — Claude API Client Service

**Epic:** Epic 4 — AI-Powered Quoting
**Status:** Complete

---

## User Story

**As a** developer,
**I want** a reusable service that communicates with the Claude AI API,
**so that** the application can send structured quoting requests and receive AI-generated estimates.

---

## Implementation

### IAIQuoteService (`Core/Interfaces/IAIQuoteService.cs`)

```csharp
Task<(AIQuoteResponse? Response, string? Error, string PromptSnapshot)>
    GenerateQuoteAsync(AIQuoteRequest request);
```

Returns a tuple: parsed response (or null), error message (or null), and prompt snapshot JSON.

### ClaudeAIQuoteService (`Infrastructure/Services/ClaudeAIQuoteService.cs`)

- Uses `IHttpClientFactory` with a named client (registered via `builder.Services.AddHttpClient("ClaudeAI")` + `builder.Services.AddScoped<IAIQuoteService, ClaudeAIQuoteService>()`). Service retrieves the client via `_httpClientFactory.CreateClient("ClaudeAI")`.
- API endpoint: `https://api.anthropic.com/v1/messages`
- Headers: `x-api-key`, `anthropic-version: 2023-06-01`, `Content-Type: application/json`

**Configuration (User Secrets):**
```json
{
  "ClaudeAI:ApiKey": "<key>",
  "ClaudeAI:Model": "claude-sonnet-4-6",
  "ClaudeAI:MaxTokens": 1024,
  "ClaudeAI:TimeoutSeconds": 30
}
```

**Retry Logic:**
- 2 retries on HTTP 429 (rate limit)
- Exponential backoff: 2s, 4s delay between retries

**Response Parsing:**
- Extracts text content from Claude's response
- Strips markdown code fences if present (` ```json ... ``` `)
- Deserializes JSON into `AIQuoteResponse` with `System.Text.Json` (case-insensitive)
- Returns error string if parsing fails

**Prompt Snapshot:**
- Captures system prompt + user prompt + AI response as serialized JSON
- Stored in `JobEstimate.AIPromptSnapshot` for audit trail

**Error Handling:**
- Timeout -> "The AI service timed out..."
- Rate limit (after retries) -> "The AI service is busy..."
- Parse failure -> "Could not parse AI response..."
- Network failure -> catches HttpRequestException

---

## Definition of Done

- [x] IAIQuoteService interface with GenerateQuoteAsync
- [x] ClaudeAIQuoteService with IHttpClientFactory
- [x] API key from user secrets (never hardcoded)
- [x] Retry with exponential backoff on 429
- [x] Graceful error handling (timeout, rate limit, parse failure)
- [x] Prompt snapshot capture for audit
- [x] Service registered in DI
