namespace RouteX.Models
{
    public class MaintenanceEntry
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public decimal Cost { get; set; }
        public string TechnicianName { get; set; } = string.Empty;
        public MaintenanceStatus Status { get; set; }
        public int OdometerAtService { get; set; }
        public DateTime NextServiceDue { get; set; }
        public string Description { get; set; } = string.Empty;
        
        // Legacy properties for backward compatibility
        public int MaintenanceId { get; set; } // Keep for existing code
        public string UnitModel { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }

    public enum MaintenanceStatus
    {
        Pending,
        Ongoing,
        Completed,
        Overdue
    }
}
