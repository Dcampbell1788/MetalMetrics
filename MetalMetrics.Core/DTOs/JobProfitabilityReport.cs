namespace MetalMetrics.Core.DTOs;

public class JobProfitabilityReport
{
    public VarianceDetail LaborVariance { get; set; } = new();
    public VarianceDetail MaterialVariance { get; set; } = new();
    public VarianceDetail MachineVariance { get; set; } = new();
    public VarianceDetail OverheadVariance { get; set; } = new();

    public decimal TotalEstimatedCost { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal QuotedPrice { get; set; }
    public decimal ActualRevenue { get; set; }

    public decimal EstimatedMarginDollars { get; set; }
    public decimal EstimatedMarginPercent { get; set; }
    public decimal ActualMarginDollars { get; set; }
    public decimal ActualMarginPercent { get; set; }
    public decimal MarginDriftDollars { get; set; }
    public decimal MarginDriftPercent { get; set; }

    public string OverallVerdict { get; set; } = string.Empty;
    public List<string> Warnings { get; set; } = new();
}
