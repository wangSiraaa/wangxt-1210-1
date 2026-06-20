using Microsoft.EntityFrameworkCore;
using PowerSwitchApi.Models;

namespace PowerSwitchApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PowerSwitchRequest> PowerSwitchRequests => Set<PowerSwitchRequest>();
    public DbSet<AffectedDevice> AffectedDevices => Set<AffectedDevice>();
    public DbSet<SwitchStep> SwitchSteps => Set<SwitchStep>();
    public DbSet<AlarmRecord> AlarmRecords => Set<AlarmRecord>();
    public DbSet<RollbackRecord> RollbackRecords => Set<RollbackRecord>();
    public DbSet<BusinessImpact> BusinessImpacts => Set<BusinessImpact>();
    public DbSet<DualPowerCheckRecord> DualPowerCheckRecords => Set<DualPowerCheckRecord>();
    public DbSet<DeviceTopology> DeviceTopologies => Set<DeviceTopology>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PowerSwitchRequest>(b =>
        {
            b.HasIndex(r => r.RequestNo).IsUnique();
            b.HasIndex(r => r.Status);
            b.HasIndex(r => r.RiskWindowStart);
        });

        modelBuilder.Entity<AffectedDevice>(b =>
        {
            b.HasIndex(d => d.RequestId);
            b.HasIndex(d => d.DeviceType);
        });

        modelBuilder.Entity<SwitchStep>(b =>
        {
            b.HasIndex(s => s.RequestId);
            b.HasIndex(s => new { s.RequestId, s.Sequence }).IsUnique();
        });

        modelBuilder.Entity<AlarmRecord>(b =>
        {
            b.HasIndex(a => a.RequestId);
            b.HasIndex(a => a.IsConfirmed);
            b.HasIndex(a => a.Severity);
        });

        modelBuilder.Entity<RollbackRecord>(b =>
        {
            b.HasIndex(r => r.RequestId);
        });

        modelBuilder.Entity<BusinessImpact>(b =>
        {
            b.HasIndex(i => i.RequestId);
        });

        modelBuilder.Entity<DualPowerCheckRecord>(b =>
        {
            b.HasIndex(c => c.RequestId);
        });

        modelBuilder.Entity<DeviceTopology>(b =>
        {
            b.HasIndex(t => t.DeviceCode).IsUnique();
            b.HasIndex(t => t.DeviceType);
            b.HasOne(t => t.Parent)
             .WithMany(t => t.Children)
             .HasForeignKey(t => t.ParentId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
