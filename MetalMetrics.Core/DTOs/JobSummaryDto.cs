namespace MetalMetrics.Core.DTOs;

public class JobSummaryDto
{
    public Guid JobId { get; set; }
    public string JobNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal ActualMarginPercent { get; set; }
    public DateTime CompletedAt { get; set; }
    public bool IsProfitable { get; set; }
    public decimal QuotePrice { get; set; }
    public string Status { get; set; } = string.Empty;
}
