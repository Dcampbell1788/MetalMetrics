namespace MetalMetrics.Core.DTOs;

public class DashboardKpiDto
{
    public int TotalJobs { get; set; }
    public int JobsThisMonth { get; set; }
    public decimal AverageMarginPercent { get; set; }
    public int JobsOverBudget { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal TargetMarginPercent { get; set; }
    public decimal TotalRevenue { get; set; }
    public int InProgressCount { get; set; }
    public int QuotedCount { get; set; }
    public decimal InProgressEstimatedValue { get; set; }
}
