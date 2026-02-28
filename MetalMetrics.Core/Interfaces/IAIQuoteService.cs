using MetalMetrics.Core.DTOs;

namespace MetalMetrics.Core.Interfaces;

public interface IAIQuoteService
{
    Task<(AIQuoteResponse? Response, string? Error, string PromptSnapshot)> GenerateQuoteAsync(AIQuoteRequest request);
}
