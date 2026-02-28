namespace MetalMetrics.Core.Entities;

public class Tenant : BaseEntity
{
    public string CompanyName { get; set; } = string.Empty;

    public ICollection<AppUser> Users { get; set; } = new List<AppUser>();
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public TenantSettings? Settings { get; set; }
}
