namespace MetalMetrics.Core.DTOs;

public class AtRiskJobDto
{
    public Guid JobId { get; set; }
    public string JobNumber { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public decimal ActualCost { get; set; }
    public decimal OveragePercent { get; set; }
}
