namespace MetalMetrics.Core.Entities;

public class JobEstimate : BaseEntity
{
    public Guid JobId { get; set; }
    public decimal EstimatedLaborHours { get; set; }
    public decimal LaborRate { get; set; }
    public decimal EstimatedMaterialCost { get; set; }
    public decimal EstimatedMachineHours { get; set; }
    public decimal MachineRate { get; set; }
    public decimal OverheadPercent { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal QuotePrice { get; set; }
    public decimal EstimatedMarginPercent { get; set; }
    public bool AIGenerated { get; set; }
    public string? AIPromptSnapshot { get; set; }
    public string? CreatedBy { get; set; }

    public Job? Job { get; set; }
}
