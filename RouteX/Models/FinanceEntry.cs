using System;
using System.ComponentModel.DataAnnotations;

namespace RouteX.Models
{
    public class FinanceEntry
    {
        public int Id { get; set; }
        
        [Required]
        public int VehicleId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ExpenseType { get; set; } = string.Empty;
        
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }
        
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int? ReferenceId { get; set; } // For Fuel ID or Maintenance ID
        
        public string? AttachmentPath { get; set; }
        
        public bool IsArchived { get; set; } = false;
        
        // Navigation properties
        public Vehicle? Vehicle { get; set; }
        
        // Legacy properties for backward compatibility
        public string PlateNumber => Vehicle?.PlateNumber ?? "Unknown";
        public string UnitModel => Vehicle?.UnitModel ?? "Unknown";
    }
}
