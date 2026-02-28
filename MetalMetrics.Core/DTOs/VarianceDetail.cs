namespace MetalMetrics.Core.DTOs;

public class VarianceDetail
{
    public decimal EstimatedAmount { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal VarianceDollars { get; set; }
    public decimal VariancePercent { get; set; }
}
