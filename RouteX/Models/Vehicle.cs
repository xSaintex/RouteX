using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RouteX.Models
{
    public class Vehicle
    {
        [Key]
        [Column("VehicleId")]
        public int Id { get; set; }
        public string UnitModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; } = VehicleStatus.Active;
        public int Mileage { get; set; }
        public bool IsArchived { get; set; }
    }

    public enum VehicleStatus
    {
        Active,
        Maintenance,
        Inactive,
        Archived
    }
}
