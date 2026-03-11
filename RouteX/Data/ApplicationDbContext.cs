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
        public DbSet<Branch> Branches { get; set; } = null!;

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

            modelBuilder.Entity<RouteTrip>(entity =>
            {
                entity.ToTable("RouteTrips");
                entity.Property(e => e.StartAddress).HasMaxLength(256);
                entity.Property(e => e.EndAddress).HasMaxLength(256);
                entity.Property(e => e.DistanceKm).HasPrecision(18, 2);
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Vehicle)
                      .WithMany()
                      .HasForeignKey(e => e.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.RouteTrips)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Branch entity configuration
            modelBuilder.Entity<Branch>(entity =>
            {
                entity.ToTable("Branches");
                entity.HasKey(e => e.BranchId);
                entity.Property(e => e.BranchName).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.Province).HasMaxLength(100);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.PhoneNumber).HasMaxLength(50);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Latitude).HasPrecision(9, 6);
                entity.Property(e => e.Longitude).HasPrecision(9, 6);
                entity.Property(e => e.CoverageRadiusKm).HasPrecision(18, 2);
                entity.Property(e => e.ManagerName).HasMaxLength(100);
                entity.Property(e => e.OperatingHours).HasMaxLength(200);
                entity.Property(e => e.ServiceAreas).HasColumnType("text");
            });

            // Configure User-Branch relationship
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Users)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Vehicle-Branch relationship
            modelBuilder.Entity<Vehicle>(entity =>
            {
                entity.ToTable("Vehicles");
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.Vehicles)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure FuelEntry-Branch relationship
            modelBuilder.Entity<FuelEntry>(entity =>
            {
                entity.ToTable("FuelEntries");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Liters).HasPrecision(10, 2);
                entity.Property(e => e.TotalCost).HasPrecision(18, 2);
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);

                entity.HasOne(f => f.Vehicle)
                      .WithMany()
                      .HasForeignKey(f => f.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.FuelEntries)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure MaintenanceEntry-Branch relationship
            modelBuilder.Entity<MaintenanceEntry>(entity =>
            {
                entity.ToTable("MaintenanceEntries");
                entity.Property(e => e.Cost).HasPrecision(18, 2);
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Vehicle)
                      .WithMany()
                      .HasForeignKey(e => e.VehicleId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.MaintenanceEntries)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure FinanceEntry-Branch relationship
            modelBuilder.Entity<FinanceEntry>(entity =>
            {
                entity.ToTable("FinanceEntries");
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.FinanceEntries)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure BudgetEntry-Branch relationship
            modelBuilder.Entity<BudgetEntry>(entity =>
            {
                entity.ToTable("MonthlyBudget");
                entity.Property(e => e.BudgetAmount).HasPrecision(18, 2);
                entity.Property(e => e.BranchId).IsRequired(false);
                entity.HasIndex(e => e.BranchId);
                entity.HasOne(e => e.Branch)
                      .WithMany(b => b.BudgetEntries)
                      .HasForeignKey(e => e.BranchId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}





