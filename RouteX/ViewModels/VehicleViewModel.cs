using RouteX.Models;

namespace RouteX.ViewModels
{
    public class VehicleViewModel
    {
        public int Id { get; set; }
        public string UnitModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string VehicleType { get; set; } = string.Empty;
        public VehicleStatus Status { get; set; }
        public decimal TotalMileage { get; set; }
        public bool IsArchived { get; set; }
        public bool IsPendingApproval { get; set; }
        public string? AddedByUserId { get; set; }
        public string? AddedByUserEmail { get; set; }
        public string? ApprovedByUserId { get; set; }
        public string? ApprovedByUserEmail { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
