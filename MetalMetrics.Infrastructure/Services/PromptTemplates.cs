namespace MetalMetrics.Infrastructure.Services;

public static class PromptTemplates
{
    public const string SystemPrompt = """
        You are an expert sheetmetal fabrication cost estimator with 20+ years of experience in job shops.
        You estimate labor hours, material costs, machine hours, and suggest competitive quote prices.

        You MUST respond with ONLY a valid JSON object (no markdown, no code fences, no explanation outside the JSON).
        Use this exact schema:

        {
          "estimatedLaborHours": <decimal>,
          "estimatedMaterialCost": <decimal>,
          "estimatedMachineHours": <decimal>,
          "overheadPercent": <decimal>,
          "suggestedQuotePrice": <decimal>,
          "reasoning": "<string explaining your estimate>",
          "assumptions": ["<assumption 1>", "<assumption 2>"],
          "confidenceLevel": "<Low|Medium|High>"
        }

        Rules:
        - All numeric values must be positive numbers
        - Use the shop's provided rates to calculate the suggested quote price
        - Include overhead in the suggested quote price
        - Add a reasonable profit margin (15-25%) to the suggested quote price
        - List all assumptions you made
        - Set confidence based on how much information was provided
        """;

    public static string BuildUserPrompt(
        string materialType,
        string materialThickness,
        string partDimensions,
        string? sheetSize,
        int quantity,
        List<string> operations,
        string complexity,
        string? specialNotes,
        decimal laborRate,
        decimal machineRate,
        decimal overheadPercent)
    {
        var ops = operations.Count > 0 ? string.Join(", ", operations) : "None specified";
        var sheet = !string.IsNullOrWhiteSpace(sheetSize) ? sheetSize : "Standard";
        var notes = !string.IsNullOrWhiteSpace(specialNotes) ? specialNotes : "None";

        return $"""
            Please estimate the cost for the following sheetmetal fabrication job:

            MATERIAL:
            - Type: {materialType}
            - Thickness: {materialThickness}
            - Part Dimensions: {partDimensions}
            - Sheet Size: {sheet}

            JOB DETAILS:
            - Quantity: {quantity} parts
            - Operations: {ops}
            - Complexity: {complexity}
            - Special Notes: {notes}

            SHOP RATES:
            - Labor Rate: ${laborRate}/hour
            - Machine Rate: ${machineRate}/hour
            - Overhead: {overheadPercent}%

            Provide your estimate as a JSON object.
            """;
    }
}
