namespace MetalMetrics.Core.Entities;

public class JobActuals : BaseEntity
{
    public Guid JobId { get; set; }
    public decimal ActualLaborHours { get; set; }
    public decimal LaborRate { get; set; }
    public decimal ActualMaterialCost { get; set; }
    public decimal ActualMachineHours { get; set; }
    public decimal MachineRate { get; set; }
    public decimal OverheadPercent { get; set; }
    public decimal TotalActualCost { get; set; }
    public decimal ActualRevenue { get; set; }
    public string? Notes { get; set; }
    public string? EnteredBy { get; set; }

    public Job? Job { get; set; }
}
