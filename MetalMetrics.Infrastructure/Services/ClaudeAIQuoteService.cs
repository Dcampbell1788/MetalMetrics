using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MetalMetrics.Core.DTOs;
using MetalMetrics.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MetalMetrics.Infrastructure.Services;

public class ClaudeAIQuoteService : IAIQuoteService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClaudeAIQuoteService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ClaudeAIQuoteService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ClaudeAIQuoteService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<(AIQuoteResponse? Response, string? Error, string PromptSnapshot)> GenerateQuoteAsync(AIQuoteRequest request)
    {
        var userPrompt = PromptTemplates.BuildUserPrompt(
            request.MaterialType,
            request.MaterialThickness,
            request.PartDimensions,
            request.SheetSize,
            request.Quantity,
            request.Operations,
            request.Complexity,
            request.SpecialNotes,
            request.LaborRate,
            request.MachineRate,
            request.OverheadPercent);

        var promptSnapshot = JsonSerializer.Serialize(new
        {
            systemPrompt = PromptTemplates.SystemPrompt,
            userPrompt
        }, JsonOptions);

        var apiKey = _configuration["ClaudeAI:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _logger.LogWarning("Claude API key is not configured");
            return (null, "AI service is not configured. Please set up the Claude API key in settings.", promptSnapshot);
        }

        var model = _configuration["ClaudeAI:Model"] ?? "claude-sonnet-4-6";
        var maxTokens = int.TryParse(_configuration["ClaudeAI:MaxTokens"], out var mt) ? mt : 1024;
        var timeoutSeconds = int.TryParse(_configuration["ClaudeAI:TimeoutSeconds"], out var ts) ? ts : 30;

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            system = PromptTemplates.SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = userPrompt }
            }
        };

        var client = _httpClientFactory.CreateClient("ClaudeAI");
        client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("x-api-key", apiKey);
        httpRequest.Headers.Add("anthropic-version", "2023-06-01");

        const int maxRetries = 2;
        for (var attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Calling Claude API (attempt {Attempt})", attempt + 1);

                var response = await client.SendAsync(httpRequest);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Claude API returned {StatusCode}: {Body}", response.StatusCode, errorBody);

                    if ((int)response.StatusCode == 429 && attempt < maxRetries)
                    {
                        var delay = (int)Math.Pow(2, attempt + 1) * 1000;
                        await Task.Delay(delay);
                        // Rebuild request for retry
                        httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
                        {
                            Content = new StringContent(JsonSerializer.Serialize(requestBody, JsonOptions), Encoding.UTF8, "application/json")
                        };
                        httpRequest.Headers.Add("x-api-key", apiKey);
                        httpRequest.Headers.Add("anthropic-version", "2023-06-01");
                        continue;
                    }

                    return (null, $"AI service returned an error (HTTP {(int)response.StatusCode}). Please try again or create a manual quote.", promptSnapshot);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var aiResponse = ParseResponse(responseJson);

                if (aiResponse == null)
                {
                    _logger.LogWarning("Failed to parse Claude API response");
                    return (null, "Could not parse AI response. Please try again or create a manual quote.", promptSnapshot);
                }

                // Append response to snapshot
                promptSnapshot = JsonSerializer.Serialize(new
                {
                    systemPrompt = PromptTemplates.SystemPrompt,
                    userPrompt,
                    aiResponse = responseJson
                }, JsonOptions);

                return (aiResponse, null, promptSnapshot);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("Claude API request timed out (attempt {Attempt})", attempt + 1);
                if (attempt < maxRetries) continue;
                return (null, "AI service timed out. Please try again or create a manual quote.", promptSnapshot);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Claude API network error (attempt {Attempt})", attempt + 1);
                if (attempt < maxRetries) continue;
                return (null, "Could not reach AI service. Please check your connection and try again.", promptSnapshot);
            }
        }

        return (null, "AI service failed after multiple attempts. Please create a manual quote.", promptSnapshot);
    }

    private static AIQuoteResponse? ParseResponse(string responseJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(responseJson);
            var root = doc.RootElement;

            // Extract text content from Claude's response format
            var content = root.GetProperty("content");
            var textBlock = content.EnumerateArray().FirstOrDefault(c =>
                c.GetProperty("type").GetString() == "text");

            var text = textBlock.GetProperty("text").GetString();
            if (string.IsNullOrWhiteSpace(text)) return null;

            // Strip markdown code fences if present
            text = text.Trim();
            if (text.StartsWith("```"))
            {
                var firstNewline = text.IndexOf('\n');
                if (firstNewline > 0) text = text[(firstNewline + 1)..];
                if (text.EndsWith("```")) text = text[..^3];
                text = text.Trim();
            }

            return JsonSerializer.Deserialize<AIQuoteResponse>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }
}
