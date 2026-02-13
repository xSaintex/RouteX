using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RouteX.Models;
using System.Collections.Generic;
using System.Linq;

namespace RouteX.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var dashboardData = GetDashboardData();
            return View(dashboardData);
        }

        private DashboardViewModel GetDashboardData()
        {
            return new DashboardViewModel
            {
                FuelConsumptionData = GetFuelConsumptionData(),
                VehicleStatusData = GetVehicleStatusData(),
                MostActiveVehicles = GetMostActiveVehicles()
            };
        }

        private List<FuelConsumptionPoint> GetFuelConsumptionData()
        {
            // Sample data for last 30 days with realistic fuel consumption trends
            var data = new List<FuelConsumptionPoint>();
            var random = new Random();
            var baseFuelCost = 2500;
            var baseDistance = 450;
            
            for (int i = 29; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i);
                var dayOfWeek = date.DayOfWeek;
                
                // Simulate realistic fuel consumption patterns
                var weekendMultiplier = (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday) ? 1.3 : 1.0;
                var weeklyVariation = Math.Sin((i % 7) * Math.PI / 3.5) * 0.3 + 1.0;
                var trendFactor = 1.0 + (29 - i) * 0.015; // Slight upward trend
                
                var fuelCost = Math.Round(baseFuelCost * weekendMultiplier * weeklyVariation * trendFactor + random.NextDouble() * 200, 2);
                var distance = Math.Round(baseDistance * weekendMultiplier * weeklyVariation * trendFactor + random.Next(-50, 100), 0);
                
                data.Add(new FuelConsumptionPoint
                {
                    Date = date.ToString("MMM dd"),
                    TotalFuelSpend = fuelCost,
                    DistanceTraveled = (int)distance
                });
            }
            
            return data;
        }

        private List<VehicleStatusData> GetVehicleStatusData()
        {
            return new List<VehicleStatusData>
            {
                new VehicleStatusData { Status = "Active", Count = 18, Percentage = 45 },
                new VehicleStatusData { Status = "Maintenance", Count = 8, Percentage = 20 },
                new VehicleStatusData { Status = "Inactive", Count = 14, Percentage = 35 }
            };
        }

        private List<ActiveVehicleData> GetMostActiveVehicles()
        {
            return new List<ActiveVehicleData>
            {
                new ActiveVehicleData { PlateNumber = "TRK-001", UnitModel = "Toyota Hilux 2023", DistanceKm = 3420, Rank = 1 },
                new ActiveVehicleData { PlateNumber = "TRK-003", UnitModel = "Isuzu NQR 2022", DistanceKm = 2890, Rank = 2 },
                new ActiveVehicleData { PlateNumber = "TRK-005", UnitModel = "Mitsubishi Fuso 2021", DistanceKm = 2650, Rank = 3 },
                new ActiveVehicleData { PlateNumber = "TRK-002", UnitModel = "Hino 300 2023", DistanceKm = 2340, Rank = 4 },
                new ActiveVehicleData { PlateNumber = "TRK-004", UnitModel = "Nissan UD 2022", DistanceKm = 1980, Rank = 5 }
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    // Dashboard View Models
    public class DashboardViewModel
    {
        public List<FuelConsumptionPoint> FuelConsumptionData { get; set; } = null!;
        public List<VehicleStatusData> VehicleStatusData { get; set; } = null!;
        public List<ActiveVehicleData> MostActiveVehicles { get; set; } = null!;
    }

    public class FuelConsumptionPoint
    {
        public string Date { get; set; } = string.Empty;
        public double TotalFuelSpend { get; set; }
        public int DistanceTraveled { get; set; }
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
}
