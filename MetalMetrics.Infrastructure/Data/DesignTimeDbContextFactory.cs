using MetalMetrics.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MetalMetrics.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=metalmetrics.db");

        return new AppDbContext(optionsBuilder.Options, new StubTenantProvider());
    }

    private class StubTenantProvider : ITenantProvider
    {
        public Guid TenantId => Guid.Empty;
    }
}
