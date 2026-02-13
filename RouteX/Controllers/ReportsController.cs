using Microsoft.AspNetCore.Mvc;
using RouteX.Models;

namespace RouteX.Controllers
{
    public class ReportsController : Controller
    {
        public IActionResult ReportsPage()
        {
            ViewData["Title"] = "Reports";

            // Initialize report data
            var viewModel = new ReportsViewModel
            {
                ReportTypes = new List<string> { "Expenses", "Fuel", "Maintenance", "Vehicle Summary" },
                Vehicles = new List<string> { "All Vehicles", "TRK-001 - Isuzu N-Series", "TRK-002 - Hino 300", "VAN-001 - Toyota Hiace", "VAN-002 - Nissan NV200" },
                SelectedReportType = "Expenses",
                SelectedVehicle = "All Vehicles",
                SelectedDateRange = "Last 30 Days",
                
                // Summary data
                TotalExpenses = 45678.90m,
                TotalFuelCost = 12345.67m,
                TotalMaintenanceCost = 8765.43m,
                TotalTrips = 156,
                
                // Report data
                ReportRecords = GetSampleReportData()
            };

            return View(viewModel);
        }

        private List<ReportRecord> GetSampleReportData()
        {
            return new List<ReportRecord>
            {
                new ReportRecord { Id = 1, Vehicle = "TRK-001 - Isuzu N-Series", Category = "Fuel", Amount = 1250.50m, Date = DateTime.Now.AddDays(-5), Description = "Diesel fuel - 45L" },
                new ReportRecord { Id = 2, Vehicle = "TRK-002 - Hino 300", Category = "Maintenance", Amount = 890.00m, Date = DateTime.Now.AddDays(-3), Description = "Oil change and filter replacement" },
                new ReportRecord { Id = 3, Vehicle = "VAN-001 - Toyota Hiace", Category = "Fuel", Amount = 980.75m, Date = DateTime.Now.AddDays(-2), Description = "Gasoline - 65L" },
                new ReportRecord { Id = 4, Vehicle = "TRK-001 - Isuzu N-Series", Category = "Maintenance", Amount = 2340.00m, Date = DateTime.Now.AddDays(-1), Description = "Brake system repair" },
                new ReportRecord { Id = 5, Vehicle = "VAN-002 - Nissan NV200", Category = "Fuel", Amount = 750.25m, Date = DateTime.Now.AddDays(-1), Description = "Gasoline - 50L" },
                new ReportRecord { Id = 6, Vehicle = "TRK-002 - Hino 300", Category = "Other", Amount = 2500.00m, Date = DateTime.Now.AddDays(-7), Description = "Annual insurance premium" },
                new ReportRecord { Id = 7, Vehicle = "VAN-001 - Toyota Hiace", Category = "Other", Amount = 450.00m, Date = DateTime.Now.AddDays(-10), Description = "Vehicle registration renewal" },
                new ReportRecord { Id = 8, Vehicle = "TRK-001 - Isuzu N-Series", Category = "Other", Amount = 320.00m, Date = DateTime.Now.AddDays(-4), Description = "Tire replacement" }
            };
        }
    }

    public class ReportsViewModel
    {
        public List<string> ReportTypes { get; set; } = new List<string>();
        public List<string> Vehicles { get; set; } = new List<string>();
        public string SelectedReportType { get; set; } = string.Empty;
        public string SelectedVehicle { get; set; } = string.Empty;
        public string SelectedDateRange { get; set; } = string.Empty;
        public decimal TotalExpenses { get; set; } = 0;
        public decimal TotalFuelCost { get; set; } = 0;
        public decimal TotalMaintenanceCost { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;
        public List<ReportRecord> ReportRecords { get; set; } = new List<ReportRecord>();
    }

    public class ReportRecord
    {
        public int Id { get; set; }
        public string Vehicle { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; } = 0;
        public DateTime Date { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }
}
