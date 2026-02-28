using System.Security.Claims;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Http;

namespace MetalMetrics.Infrastructure.Services;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("TenantId");
            return claim is not null && Guid.TryParse(claim.Value, out var tenantId)
                ? tenantId
                : Guid.Empty;
        }
    }
}
