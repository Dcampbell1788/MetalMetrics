using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Enums;
using MetalMetrics.Core.Interfaces;
using MetalMetrics.Infrastructure.Data;
using MetalMetrics.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Tests.Infrastructure;

[TestClass]
public class JobServiceTests
{
    private class FakeTenantProvider : ITenantProvider
    {
        public Guid TenantId { get; set; } = Guid.NewGuid();
    }

    private static (AppDbContext context, JobService service, FakeTenantProvider tenant) CreateContext()
    {
        var tenant = new FakeTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options, tenant);
        context.Database.EnsureCreated();
        var service = new JobService(context, tenant);
        return (context, service, tenant);
    }

    [TestMethod]
    public async Task CreateAsync_GeneratesJobNumber()
    {
        var (context, service, _) = CreateContext();

        var job = await service.CreateAsync("Test Customer", "Test job");

        Assert.AreEqual("JOB-0001", job.JobNumber);
        Assert.AreEqual("Test Customer", job.CustomerName);
    }

    [TestMethod]
    public async Task CreateAsync_IncrementsJobNumber()
    {
        var (context, service, _) = CreateContext();

        await service.CreateAsync("Customer 1", null);
        var job2 = await service.CreateAsync("Customer 2", null);

        Assert.AreEqual("JOB-0002", job2.JobNumber);
    }

    [TestMethod]
    public async Task GetAllAsync_FiltersByStatus()
    {
        var (context, service, _) = CreateContext();

        var job1 = await service.CreateAsync("Customer 1", null);
        var job2 = await service.CreateAsync("Customer 2", null);
        job2.Status = JobStatus.InProgress;
        await service.UpdateAsync(job2);

        var quoted = await service.GetAllAsync(statusFilter: JobStatus.Quoted);
        var inProgress = await service.GetAllAsync(statusFilter: JobStatus.InProgress);

        Assert.AreEqual(1, quoted.Count);
        Assert.AreEqual(1, inProgress.Count);
    }

    [TestMethod]
    public async Task GetAllAsync_SearchesByCustomerName()
    {
        var (context, service, _) = CreateContext();

        await service.CreateAsync("Acme Fabrication", null);
        await service.CreateAsync("Smith Welding", null);

        var results = await service.GetAllAsync(search: "acme");

        Assert.AreEqual(1, results.Count);
        Assert.AreEqual("Acme Fabrication", results[0].CustomerName);
    }

    [TestMethod]
    public async Task StatusTransition_QuotedToInProgressToCompleted()
    {
        var (context, service, _) = CreateContext();

        var job = await service.CreateAsync("Test Customer", null);
        Assert.AreEqual(JobStatus.Quoted, job.Status);

        job.Status = JobStatus.InProgress;
        await service.UpdateAsync(job);
        var updated = await service.GetByIdAsync(job.Id);
        Assert.AreEqual(JobStatus.InProgress, updated!.Status);

        job.Status = JobStatus.Completed;
        job.CompletedAt = DateTime.UtcNow;
        await service.UpdateAsync(job);
        updated = await service.GetByIdAsync(job.Id);
        Assert.AreEqual(JobStatus.Completed, updated!.Status);
        Assert.IsNotNull(updated.CompletedAt);

        job.Status = JobStatus.Invoiced;
        await service.UpdateAsync(job);
        updated = await service.GetByIdAsync(job.Id);
        Assert.AreEqual(JobStatus.Invoiced, updated!.Status);
    }
}
