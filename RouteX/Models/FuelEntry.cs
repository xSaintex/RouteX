using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RouteX.Models
{
    public class FuelEntry
    {
        public int Id { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Please select a vehicle.")]
        public int VehicleId { get; set; }
        
        // Navigation property
        public Vehicle? Vehicle { get; set; }
        
        [Required(ErrorMessage = "Driver name is required.")]
        public string Driver { get; set; } = string.Empty;
        [Required(ErrorMessage = "Date and time is required.")]
        public DateTime DateTime { get; set; }
        [Required(ErrorMessage = "Fuel station is required.")]
        public string FuelStation { get; set; } = string.Empty;
        [Range(0, int.MaxValue, ErrorMessage = "Odometer must be a positive number.")]
        public int Odometer { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Liters must be greater than 0.")]
        public decimal Liters { get; set; }
        [Range(0.01, double.MaxValue, ErrorMessage = "Total cost must be greater than 0.")]
        public decimal TotalCost { get; set; }
        [Required(ErrorMessage = "Fuel type is required.")]
        public string FuelType { get; set; } = string.Empty;
        public bool FullTank { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        
        // Legacy properties for backward compatibility
        public string UnitModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public enum FuelEntryStatus
    {
        Active,
        Archived
    }
}
