# Feature 4.1 — Claude API Client Service

**Epic:** Epic 4 — AI-Powered Quoting (Claude Integration)
**Status:** Pending
**Priority:** High
**Estimated Effort:** Medium

---

## User Story

**As a** developer,
**I want** a reusable service that communicates with the Claude AI API,
**so that** the application can send structured quoting requests and receive AI-generated estimates.

---

## Acceptance Criteria

- [ ] `IAIQuoteService` interface defined in `MetalMetrics.Core`
- [ ] `ClaudeAIQuoteService` implementation created in `MetalMetrics.Infrastructure`
- [ ] API key read from `appsettings.json` or user secrets (never hardcoded)
- [ ] Service sends structured prompts and parses structured JSON responses
- [ ] Rate limiting implemented to avoid API throttling
- [ ] Graceful error handling for: API timeout, rate limit exceeded, invalid response, network failure
- [ ] Fallback behavior: if AI fails, user can still manually create a quote
- [ ] Service registered in DI container
- [ ] HTTP client uses `IHttpClientFactory` for proper lifecycle management

---

## Technical Notes

- Interface: `MetalMetrics.Core/Interfaces/IAIQuoteService.cs`
- Implementation: `MetalMetrics.Infrastructure/Services/ClaudeAIQuoteService.cs`
- Use `IHttpClientFactory` with a named client for Claude API
- API endpoint: `https://api.anthropic.com/v1/messages`
- Configuration in `appsettings.json`:
  ```json
  {
    "ClaudeAI": {
      "ApiKey": "(from user secrets)",
      "Model": "claude-sonnet-4-6",
      "MaxTokens": 1024,
      "TimeoutSeconds": 30
    }
  }
  ```
- Request/response DTOs:
  - `AIQuoteRequest`: material info, dimensions, operations, complexity, tenant rates
  - `AIQuoteResponse`: estimated hours/costs, suggested price, reasoning, confidence
- Consider retry with exponential backoff for transient failures
- Log all API calls for debugging (but redact sensitive data)

---

## Dependencies

- Feature 1.1 (Solution structure — Infrastructure project)

---

## Definition of Done

- [ ] `IAIQuoteService` interface defined with clear method signatures
- [ ] `ClaudeAIQuoteService` implementation communicates with Claude API
- [ ] API key loaded from configuration (not hardcoded)
- [ ] Error handling covers timeout, rate limit, and invalid response scenarios
- [ ] Service registered in DI
- [ ] At least 1 unit test (mocked HTTP client)
- [ ] Manual test: send a quote request and receive a valid response
