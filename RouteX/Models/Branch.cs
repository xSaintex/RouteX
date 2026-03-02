using System.ComponentModel.DataAnnotations;

namespace RouteX.Models
{
    public class Branch
    {
        [Key]
        public int BranchId { get; set; }

        [Required]
        [StringLength(100)]
        public string BranchName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(100)]
        public string Province { get; set; } = string.Empty;

        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public decimal CoverageRadiusKm { get; set; }

        public BranchStatus Status { get; set; } = BranchStatus.Active;

        [StringLength(100)]
        public string ManagerName { get; set; } = string.Empty;

        [StringLength(200)]
        public string OperatingHours { get; set; } = string.Empty;

        public string ServiceAreas { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }

        public bool IsArchived { get; set; } = false;

        // Navigation properties - 1 to Many relationships
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
        public ICollection<FuelEntry> FuelEntries { get; set; } = new List<FuelEntry>();
        public ICollection<MaintenanceEntry> MaintenanceEntries { get; set; } = new List<MaintenanceEntry>();
        public ICollection<FinanceEntry> FinanceEntries { get; set; } = new List<FinanceEntry>();
        public ICollection<BudgetEntry> BudgetEntries { get; set; } = new List<BudgetEntry>();
        public ICollection<RouteTrip> RouteTrips { get; set; } = new List<RouteTrip>();
    }

    public enum BranchStatus
    {
        Active = 0,
        Inactive = 1,
        Archived = 2
    }
}
