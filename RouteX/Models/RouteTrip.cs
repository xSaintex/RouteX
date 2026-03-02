using System.ComponentModel.DataAnnotations;

namespace RouteX.Models
{
    public class RouteTrip
    {
        [Key]
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public string StartAddress { get; set; } = string.Empty;
        public string EndAddress { get; set; } = string.Empty;
        public decimal? DistanceKm { get; set; }
        public RouteTripStatus Status { get; set; } = RouteTripStatus.Saved;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool IsArchived { get; set; } = false;
        public DateTime? ArchivedAt { get; set; }
    }

    public enum RouteTripStatus
    {
        Saved = 0,
        Completed = 1,
        Cancelled = 2
    }
}
