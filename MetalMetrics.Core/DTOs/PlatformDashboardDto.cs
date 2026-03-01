namespace MetalMetrics.Core.DTOs;

public class PlatformDashboardDto
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TrialTenants { get; set; }
    public int PastDueTenants { get; set; }
    public int CancelledTenants { get; set; }
    public decimal MRR { get; set; }
    public int TotalUsers { get; set; }
    public int TotalJobs { get; set; }
}
