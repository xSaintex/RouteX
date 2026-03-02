using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace RouteX.Models
{
    public class MaintenanceEntry
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public string? PlateNumber { get; set; }
        public string? ServiceType { get; set; }
        [NotMapped]
        public DateTime ServiceDate
        {
            get => Date ?? default;
            set => Date = value;
        }
        public decimal? Cost { get; set; }
        public string? TechnicianName { get; set; }
        public int? OdometerAtService { get; set; }
        public DateTime? NextServiceDue { get; set; }
        public string? Description { get; set; }
        public bool? IsArchived { get; set; }
        public int? Status { get; set; } // Use int instead of enum to match database schema

        // Navigation property
        public Vehicle? Vehicle { get; set; }

        // Legacy properties for backward compatibility
        public int? MaintenanceId { get; set; } // Keep for existing code
        public string? UnitModel { get; set; }
        public DateTime? Date { get; set; }

        // Branch relationship - MaintenanceEntry belongs to one branch
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }

    public enum MaintenanceStatus
    {
        Pending = 0,
        Ongoing = 1,
        Completed = 2,
        Overdue = 3,
        Archived = 4
    }
}
