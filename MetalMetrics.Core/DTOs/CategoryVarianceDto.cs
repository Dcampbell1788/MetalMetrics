namespace MetalMetrics.Core.DTOs;

public class CategoryVarianceDto
{
    public string Category { get; set; } = string.Empty;
    public decimal AverageVariancePercent { get; set; }
    public string Direction { get; set; } = string.Empty;
    public int JobCount { get; set; }
}
