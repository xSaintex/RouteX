using System.ComponentModel.DataAnnotations;

namespace RouteX.ViewModels
{
    public class DashboardViewModel
    {
        public List<FuelConsumptionPoint> FuelConsumptionData { get; set; } = new();
        public List<VehicleStatusData> VehicleStatusData { get; set; } = new();
        public List<ActiveVehicleData> MostActiveVehicles { get; set; } = new();
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
        
        [Display(Name = "Total Vehicles")]
        public int TotalVehicles { get; set; }
        
        [Display(Name = "Active Vehicles")]
        public int ActiveVehicles { get; set; }
        
        [Display(Name = "Pending Maintenance")]
        public int PendingMaintenance { get; set; }
        
        [Display(Name = "Total Expenses")]
        public decimal TotalExpenses { get; set; }
        
        [Display(Name = "Total Trips")]
        public int TotalTrips { get; set; }
        
        [Display(Name = "Total Fuel Cost")]
        public decimal TotalFuelCost { get; set; }
    }

    public class FuelConsumptionPoint
    {
        public string Date { get; set; } = string.Empty;
        public double TotalFuelSpend { get; set; }
        public decimal DistanceTraveled { get; set; }
    }

    public class VehicleStatusData
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Percentage { get; set; }
    }

    public class ActiveVehicleData
    {
        public int Rank { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
        public string UnitModel { get; set; } = string.Empty;
        public int DistanceKm { get; set; }
    }

    public class RecentActivityItem
    {
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public DateTime ActionDate { get; set; }
        
        public string GetRelativeTime()
        {
            var timeSpan = DateTime.UtcNow - ActionDate;
            
            if (timeSpan.TotalMinutes < 1)
                return "Just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} minute{(timeSpan.TotalMinutes >= 2 ? "s" : "")} ago";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} hour{(timeSpan.TotalHours >= 2 ? "s" : "")} ago";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} day{(timeSpan.TotalDays >= 2 ? "s" : "")} ago";
            
            return ActionDate.ToString("MMM dd, yyyy");
        }
    }
}
