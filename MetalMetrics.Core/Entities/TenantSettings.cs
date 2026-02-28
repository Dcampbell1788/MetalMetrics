namespace MetalMetrics.Core.Entities;

public class TenantSettings : BaseEntity
{
    public decimal DefaultLaborRate { get; set; } = 75m;
    public decimal DefaultMachineRate { get; set; } = 150m;
    public decimal DefaultOverheadPercent { get; set; } = 15m;
    public decimal TargetMarginPercent { get; set; } = 20m;

    public Tenant? Tenant { get; set; }
}
