using MetalMetrics.Core.Enums;
using Microsoft.AspNetCore.Identity;

namespace MetalMetrics.Core.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public AppRole Role { get; set; } = AppRole.Journeyman;

    public Tenant? Tenant { get; set; }
}
