using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RouteX.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RouteX.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ================== DbSets ==================
        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<FuelEntry> FuelEntries { get; set; } = null!;
        public DbSet<MaintenanceEntry> MaintenanceEntries { get; set; } = null!;
        public DbSet<FinanceEntry> FinanceEntries { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public new DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure AuditLog
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.ActionDate).IsRequired();
                entity.ToTable("AuditLogs", table => table.ExcludeFromMigrations());
            });

            // Configure FuelEntry decimal properties
            modelBuilder.Entity<FuelEntry>(entity =>
            {
                entity.ToTable("FuelEntries", table => table.ExcludeFromMigrations());
                entity.Property(e => e.Liters).HasPrecision(10, 2);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            });

            // Configure MaintenanceEntry decimal property
            modelBuilder.Entity<MaintenanceEntry>(entity =>
            {
                entity.ToTable("MaintenanceEntries", table => table.ExcludeFromMigrations());
                entity.Property(e => e.Cost).HasPrecision(18, 2);
            });

            // Configure FinanceEntry decimal property
            modelBuilder.Entity<FinanceEntry>(entity =>
            {
                entity.ToTable("FinanceEntries", table => table.ExcludeFromMigrations());
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles", table => table.ExcludeFromMigrations());
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", table => table.ExcludeFromMigrations());
            });
        }
    }
}
