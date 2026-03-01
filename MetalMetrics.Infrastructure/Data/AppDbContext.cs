using MetalMetrics.Core.Entities;
using MetalMetrics.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MetalMetrics.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole, string>
{
    private readonly ITenantProvider _tenantProvider;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobEstimate> JobEstimates => Set<JobEstimate>();
    public DbSet<JobActuals> JobActuals => Set<JobActuals>();
    public DbSet<JobAssignment> JobAssignments => Set<JobAssignment>();
    public DbSet<JobTimeEntry> JobTimeEntries => Set<JobTimeEntry>();
    public DbSet<JobNote> JobNotes => Set<JobNote>();
    public DbSet<JobAttachment> JobAttachments => Set<JobAttachment>();
    public DbSet<PlatformPlan> PlatformPlans => Set<PlatformPlan>();
    public DbSet<SubscriptionEvent> SubscriptionEvents => Set<SubscriptionEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.Property(t => t.CompanyName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.StripeCustomerId).HasMaxLength(255);
            entity.Property(t => t.StripeSubscriptionId).HasMaxLength(255);
            entity.Property(t => t.StripePriceId).HasMaxLength(255);

            entity.HasIndex(t => t.StripeCustomerId)
                .IsUnique()
                .HasFilter("[StripeCustomerId] IS NOT NULL");

            entity.HasOne(t => t.Settings)
                .WithOne(s => s.Tenant)
                .HasForeignKey<TenantSettings>(s => s.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(u => u.TenantId);

            entity.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantSettings>(entity =>
        {
            entity.Property(s => s.DefaultLaborRate).HasColumnType("decimal(18,2)");
            entity.Property(s => s.DefaultMachineRate).HasColumnType("decimal(18,2)");
            entity.Property(s => s.DefaultOverheadPercent).HasColumnType("decimal(5,2)");
            entity.Property(s => s.TargetMarginPercent).HasColumnType("decimal(5,2)");
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.Property(j => j.JobNumber).IsRequired().HasMaxLength(20);
            entity.Property(j => j.Slug).IsRequired().HasMaxLength(8);
            entity.Property(j => j.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(j => j.Description).HasMaxLength(2000);
            entity.HasIndex(j => new { j.TenantId, j.JobNumber }).IsUnique();
            entity.HasIndex(j => new { j.TenantId, j.Slug }).IsUnique();

            entity.HasOne(j => j.Tenant)
                .WithMany(t => t.Jobs)
                .HasForeignKey(j => j.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(j => j.Estimate)
                .WithOne(e => e.Job)
                .HasForeignKey<JobEstimate>(e => e.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(j => j.Actuals)
                .WithOne(a => a.Job)
                .HasForeignKey<JobActuals>(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobEstimate>(entity =>
        {
            entity.Property(e => e.EstimatedLaborHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.LaborRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.EstimatedMaterialCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.EstimatedMachineHours).HasColumnType("decimal(18,2)");
            entity.Property(e => e.MachineRate).HasColumnType("decimal(18,2)");
            entity.Property(e => e.OverheadPercent).HasColumnType("decimal(5,2)");
            entity.Property(e => e.TotalEstimatedCost).HasColumnType("decimal(18,2)");
            entity.Property(e => e.QuotePrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.EstimatedMarginPercent).HasColumnType("decimal(5,2)");
        });

        modelBuilder.Entity<JobActuals>(entity =>
        {
            entity.Property(a => a.ActualLaborHours).HasColumnType("decimal(18,2)");
            entity.Property(a => a.LaborRate).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ActualMaterialCost).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ActualMachineHours).HasColumnType("decimal(18,2)");
            entity.Property(a => a.MachineRate).HasColumnType("decimal(18,2)");
            entity.Property(a => a.OverheadPercent).HasColumnType("decimal(5,2)");
            entity.Property(a => a.TotalActualCost).HasColumnType("decimal(18,2)");
            entity.Property(a => a.ActualRevenue).HasColumnType("decimal(18,2)");
            entity.Property(a => a.Notes).HasMaxLength(2000);
        });

        modelBuilder.Entity<JobAssignment>(entity =>
        {
            entity.HasIndex(a => new { a.JobId, a.UserId }).IsUnique();
            entity.HasIndex(a => a.UserId);

            entity.HasOne(a => a.Job)
                .WithMany(j => j.Assignments)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobTimeEntry>(entity =>
        {
            entity.HasIndex(t => new { t.JobId, t.UserId });

            entity.Property(t => t.HoursWorked).HasColumnType("decimal(18,2)");
            entity.Property(t => t.Notes).HasMaxLength(2000);

            entity.HasOne(t => t.Job)
                .WithMany(j => j.TimeEntries)
                .HasForeignKey(t => t.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<JobNote>(entity =>
        {
            entity.HasIndex(n => n.JobId);

            entity.Property(n => n.AuthorName).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Content).IsRequired().HasMaxLength(2000);
            entity.Property(n => n.ImageFileName).HasMaxLength(500);

            entity.HasOne(n => n.Job)
                .WithMany(j => j.Notes)
                .HasForeignKey(n => n.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<JobAttachment>(entity =>
        {
            entity.HasIndex(a => a.JobId);
            entity.Property(a => a.FileName).IsRequired().HasMaxLength(500);
            entity.Property(a => a.StoredFileName).IsRequired().HasMaxLength(500);
            entity.Property(a => a.ContentType).HasMaxLength(200);
            entity.Property(a => a.Category).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Description).HasMaxLength(500);
            entity.Property(a => a.UploadedByName).HasMaxLength(200);

            entity.HasOne(a => a.Job)
                .WithMany(j => j.Attachments)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlatformPlan>(entity =>
        {
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.StripePriceId).HasMaxLength(255);
            entity.Property(p => p.StripeProductId).HasMaxLength(255);

            entity.HasIndex(p => p.StripePriceId)
                .IsUnique()
                .HasFilter("[StripePriceId] IS NOT NULL");
        });

        modelBuilder.Entity<SubscriptionEvent>(entity =>
        {
            entity.HasIndex(e => e.TenantId);
            entity.Property(e => e.EventType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.StripeEventId).HasMaxLength(255);
            entity.Property(e => e.Details).HasMaxLength(4000);

            entity.HasIndex(e => e.StripeEventId)
                .IsUnique()
                .HasFilter("[StripeEventId] IS NOT NULL");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAt == default)
                    entry.Entity.CreatedAt = now;
                if (entry.Entity.UpdatedAt == default)
                    entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = _tenantProvider.TenantId;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
