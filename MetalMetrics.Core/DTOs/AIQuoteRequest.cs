namespace MetalMetrics.Core.DTOs;

public class AIQuoteRequest
{
    public string MaterialType { get; set; } = string.Empty;
    public string MaterialThickness { get; set; } = string.Empty;
    public string PartDimensions { get; set; } = string.Empty;
    public string? SheetSize { get; set; }
    public int Quantity { get; set; }
    public List<string> Operations { get; set; } = new();
    public string Complexity { get; set; } = "Moderate";
    public string? SpecialNotes { get; set; }
    public decimal LaborRate { get; set; }
    public decimal MachineRate { get; set; }
    public decimal OverheadPercent { get; set; }
}
