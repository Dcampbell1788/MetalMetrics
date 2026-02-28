namespace MetalMetrics.Core.DTOs;

public class CustomerProfitabilityDto
{
    public string CustomerName { get; set; } = string.Empty;
    public int JobCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal MarginPercent { get; set; }
}
