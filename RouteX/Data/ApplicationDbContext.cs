using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RouteX.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
namespace RouteX.Data

{
    public class ApplicationDbContext : IdentityDbContext
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)

            : base(options)

        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    optionsBuilder.UseSqlServer(connectionString,
                        sqlOptions => sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null));
                }

            }

            // Suppress pending model changes warning to prevent migration issues with existing tables
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<FuelEntry> FuelEntries { get; set; } = null!;
        public DbSet<MaintenanceEntry> MaintenanceEntries { get; set; } = null!;
        public DbSet<FinanceEntry> FinanceEntries { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public new DbSet<User> Users { get; set; } = null!;
        public DbSet<BudgetEntry> BudgetEntries { get; set; } = null!;
        public DbSet<RouteTrip> RouteTrips { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.AuditLogId);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.Action).IsRequired();
                entity.Property(e => e.ActionDate).IsRequired();
                entity.ToTable("AuditLogs");

            });

            modelBuilder.Entity<FuelEntry>(entity =>
            {
                entity.ToTable("FuelEntries");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Liters).HasPrecision(10, 2);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
                
                // Configure foreign key relationship
                entity.HasOne(f => f.Vehicle)
                      .WithMany()
                      .HasForeignKey(f => f.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaintenanceEntry>(entity =>
            {
                entity.ToTable("MaintenanceEntries");
                entity.Property(e => e.Cost).HasPrecision(18, 2);
                entity.HasOne(e => e.Vehicle)
                      .WithMany()
                      .HasForeignKey(e => e.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<FinanceEntry>(entity =>
            {
                entity.ToTable("FinanceEntries");
                entity.Property(e => e.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
            });

            modelBuilder.Entity<BudgetEntry>(entity =>
            {
                entity.ToTable("MonthlyBudget");

                entity.Property(e => e.BudgetAmount)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<RouteTrip>(entity =>
            {
                entity.ToTable("RouteTrips");
                entity.Property(e => e.StartAddress).HasMaxLength(256);
                entity.Property(e => e.EndAddress).HasMaxLength(256);
                entity.Property(e => e.DistanceKm).HasPrecision(18, 2);
                entity.HasOne(e => e.Vehicle)
                      .WithMany()
                      .HasForeignKey(e => e.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}





