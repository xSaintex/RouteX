using RouteX.Models;

namespace RouteX.ViewModels
{
    public class OpStaffDashboardViewModel
    {
        public List<Vehicle> ActiveVehicles { get; set; } = new();
        public List<Vehicle> IdleVehicles { get; set; } = new();
        public List<Vehicle> MaintenanceVehicles { get; set; } = new();
        public List<RouteX.Controllers.VehicleStatusData> VehicleStatusData { get; set; } = new();
        public List<Vehicle> ActiveTripsToday { get; set; } = new();
    }
}
