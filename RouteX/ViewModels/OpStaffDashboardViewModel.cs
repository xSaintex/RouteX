using RouteX.Models;

namespace RouteX.ViewModels
{
    public class OpStaffDashboardViewModel
    {
        public List<Vehicle> ActiveVehicles { get; set; } = new();
        public List<Vehicle> InactiveVehicles { get; set; } = new();
        public List<Vehicle> MaintenanceVehicles { get; set; } = new();
        public List<VehicleStatusData> VehicleStatusData { get; set; } = new();
        public List<ActiveVehicleData> MostActiveVehicles { get; set; } = new();
        public int TotalTrips { get; set; }
    }
}
