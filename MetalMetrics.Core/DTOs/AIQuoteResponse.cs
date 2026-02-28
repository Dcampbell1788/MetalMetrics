namespace MetalMetrics.Core.DTOs;

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
