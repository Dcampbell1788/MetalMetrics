using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Tests.Infrastructure;

[TestClass]
public class AppDbContextTests
{
    private class TestEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    private class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
    }

    private class TestDbContext : AppDbContext
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        public TestDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider) { }
    }

    private static (TestDbContext context, FakeTenantProvider tenant) CreateContext()
    {
        var tenant = new FakeTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new TestDbContext(options, tenant);
        context.Database.EnsureCreated();
        return (context, tenant);
    }

    [TestMethod]
    public async Task SaveChanges_SetsCreatedAtAndUpdatedAt_OnInsert()
    {
        var (context, _) = CreateContext();
        var entity = new TestEntity { Name = "Test" };

        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        Assert.AreNotEqual(default, entity.CreatedAt);
        Assert.AreNotEqual(default, entity.UpdatedAt);
        Assert.AreEqual(entity.CreatedAt, entity.UpdatedAt);
    }

    [TestMethod]
    public async Task SaveChanges_UpdatesOnlyUpdatedAt_OnModify()
    {
        var (context, _) = CreateContext();
        var entity = new TestEntity { Name = "Test" };

        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        var originalCreatedAt = entity.CreatedAt;
        var originalUpdatedAt = entity.UpdatedAt;

        // Small delay to ensure timestamp difference
        await Task.Delay(10);

        entity.Name = "Updated";
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();

        Assert.AreEqual(originalCreatedAt, entity.CreatedAt);
        Assert.IsTrue(entity.UpdatedAt > originalUpdatedAt);
    }

    [TestMethod]
    public async Task SaveChanges_AutoSetsTenantId_WhenEmpty()
    {
        var (context, tenant) = CreateContext();
        var expectedTenantId = tenant.TenantId;
        var entity = new TestEntity { Name = "Test" };

        Assert.AreEqual(Guid.Empty, entity.TenantId);

        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        Assert.AreEqual(expectedTenantId, entity.TenantId);
    }
}
