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
        public bool IsArchived { get; set; }

        // Pending approval workflow fields
        public bool IsPendingApproval { get; set; } = false;
        public string? AddedByUserId { get; set; }
        public string? AddedByUserEmail { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUserEmail { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? CreatedDate { get; set; }
    }

    public enum VehicleStatus
    {
        Active,
        Maintenance,
        Inactive,
        Archived
    }
}
