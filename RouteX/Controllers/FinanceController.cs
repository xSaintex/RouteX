using Microsoft.AspNetCore.Mvc;
using RouteX.Models;
using System.Globalization;

namespace RouteX.Controllers
{
    public class FinanceController : Controller
    {
        private static string FormatPhilippinePeso(decimal amount)
        {
            return amount.ToString("C", new CultureInfo("en-PH"));
        }
        public IActionResult FinancePage()
        {
            ViewData["Title"] = "Finance";

            // Dashboard summary data
            var summary = new FinanceSummary
            {
                TotalExpensesAllTime = 45678.90m,
                TotalFuelCost = 12345.67m,
                TotalMaintenanceCost = 8765.43m,
                AverageCostPerVehicle = 9135.78m,
                HighestCostVehicle = "Vehicle 001"
            };

            // Cost per vehicle data
            var costPerVehicle = new List<VehicleCostSummary>
            {
                new VehicleCostSummary 
                { 
                    Vehicle = "TRK-001 - Isuzu N-Series", 
                    FuelCost = 3456.78m, 
                    MaintenanceCost = 1234.56m, 
                    OtherExpenses = 567.89m, 
                    TotalCost = 5259.23m 
                },
                new VehicleCostSummary 
                { 
                    Vehicle = "VAN-002 - Toyota HiAce", 
                    FuelCost = 2345.67m, 
                    MaintenanceCost = 890.12m, 
                    OtherExpenses = 345.67m, 
                    TotalCost = 3581.46m 
                },
                new VehicleCostSummary 
                { 
                    Vehicle = "TRK-003 - Hino 300", 
                    FuelCost = 2890.34m, 
                    MaintenanceCost = 1567.89m, 
                    OtherExpenses = 234.56m, 
                    TotalCost = 4692.79m 
                },
                new VehicleCostSummary 
                { 
                    Vehicle = "CAR-004 - Toyota Corolla", 
                    FuelCost = 1234.56m, 
                    MaintenanceCost = 456.78m, 
                    OtherExpenses = 123.45m, 
                    TotalCost = 1814.79m 
                },
                new VehicleCostSummary 
                { 
                    Vehicle = "TRK-005 - Mitsubishi Canter", 
                    FuelCost = 3123.45m, 
                    MaintenanceCost = 987.65m, 
                    OtherExpenses = 456.78m, 
                    TotalCost = 4567.88m 
                }
            };

            // Expense records data (limited to 6 items)
            var expenseRecords = new List<ExpenseRecord>
            {
                new ExpenseRecord 
                { 
                    ExpenseId = 1001, 
                    Vehicle = "TRK-001 - Isuzu N-Series", 
                    ExpenseType = "Fuel", 
                    Amount = 185.75m, 
                    ExpenseDate = new DateTime(2025, 12, 10), 
                    Description = "Regular fuel fill-up", 
                    CreatedDate = new DateTime(2025, 12, 10)
                },
                new ExpenseRecord 
                { 
                    ExpenseId = 1002, 
                    Vehicle = "VAN-002 - Toyota HiAce", 
                    ExpenseType = "Maintenance", 
                    Amount = 450.00m, 
                    ExpenseDate = new DateTime(2025, 12, 08), 
                    Description = "Brake pad replacement", 
                    CreatedDate = new DateTime(2025, 12, 08)
                },
                new ExpenseRecord 
                { 
                    ExpenseId = 1003, 
                    Vehicle = "TRK-003 - Hino 300", 
                    ExpenseType = "Other", 
                    Amount = 1200.00m, 
                    ExpenseDate = new DateTime(2025, 12, 01), 
                    Description = "Annual insurance premium", 
                    CreatedDate = new DateTime(2025, 12, 01)
                },
                new ExpenseRecord 
                { 
                    ExpenseId = 1004, 
                    Vehicle = "CAR-004 - Toyota Corolla", 
                    ExpenseType = "Fuel", 
                    Amount = 350.00m, 
                    ExpenseDate = new DateTime(2025, 12, 05), 
                    Description = "Premium gasoline", 
                    CreatedDate = new DateTime(2025, 12, 05)
                },
                new ExpenseRecord 
                { 
                    ExpenseId = 1005, 
                    Vehicle = "TRK-005 - Mitsubishi Canter", 
                    ExpenseType = "Maintenance", 
                    Amount = 75.50m, 
                    ExpenseDate = new DateTime(2025, 12, 12), 
                    Description = "Oil change service", 
                    CreatedDate = new DateTime(2025, 12, 12)
                },
                new ExpenseRecord 
                { 
                    ExpenseId = 1006, 
                    Vehicle = "TRK-001 - Isuzu N-Series", 
                    ExpenseType = "Other", 
                    Amount = 220.30m, 
                    ExpenseDate = new DateTime(2025, 12, 15), 
                    Description = "Toll fees and parking", 
                    CreatedDate = new DateTime(2025, 12, 15)
                }
            };

            var viewModel = new FinanceViewModel
            {
                Summary = summary,
                CostPerVehicle = costPerVehicle,
                ExpenseRecords = expenseRecords
            };

            return View(viewModel);
        }

        public IActionResult TotalExpensesAllTime()
        {
            ViewData["Title"] = "Total Expenses (All-Time)";
            
            // Generate time series data for all-time expenses
            var timeSeriesData = GetTimeSeriesExpenseData();
            
            return View(timeSeriesData);
        }

        private List<ExpenseTimeSeriesData> GetTimeSeriesExpenseData()
        {
            var data = new List<ExpenseTimeSeriesData>();
            var random = new Random();
            
            // Generate monthly data for the last 24 months
            for (int i = 23; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var monthlyTotal = random.NextDouble() * 50000 + 10000; // Random between 10,000-60,000
                
                data.Add(new ExpenseTimeSeriesData
                {
                    Month = date.ToString("MMM yyyy"),
                    ExpenseAmount = Math.Round(monthlyTotal, 2)
                });
            }
            
            return data;
        }

        public IActionResult TotalFuel()
        {
            ViewData["Title"] = "Total Fuel Expenses";
            
            // Generate fuel expense data for different time periods
            var weeklyData = GetWeeklyFuelData();
            var monthlyData = GetMonthlyFuelData();
            var annualData = GetAnnualFuelData();
            
            // Pass data to view via ViewBag
            ViewBag.WeeklyData = weeklyData;
            ViewBag.MonthlyData = monthlyData;
            ViewBag.AnnualData = annualData;
            
            return View();
        }

        private List<FuelTimeSeriesData> GetWeeklyFuelData()
        {
            var data = new List<FuelTimeSeriesData>();
            var random = new Random();
            
            // Generate data for last 12 weeks
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddDays(-i * 7);
                var fuelCost = Math.Round(random.NextDouble() * 8000 + 2000, 2);
                
                data.Add(new FuelTimeSeriesData
                {
                    Period = date.ToString("MMM dd"),
                    FuelCost = fuelCost
                });
            }
            
            return data;
        }

        private List<FuelTimeSeriesData> GetMonthlyFuelData()
        {
            var data = new List<FuelTimeSeriesData>();
            var random = new Random();
            
            // Generate data for last 12 months
            for (int i = 11; i >= 0; i--)
            {
                var date = DateTime.Now.AddMonths(-i);
                var fuelCost = Math.Round(random.NextDouble() * 30000 + 5000, 2);
                
                data.Add(new FuelTimeSeriesData
                {
                    Period = date.ToString("MMM yyyy"),
                    FuelCost = fuelCost
                });
            }
            
            return data;
        }

        private List<FuelTimeSeriesData> GetAnnualFuelData()
        {
            var data = new List<FuelTimeSeriesData>();
            var random = new Random();
            
            // Generate data for last 5 years
            for (int i = 4; i >= 0; i--)
            {
                var year = DateTime.Now.Year - i;
                var fuelCost = Math.Round(random.NextDouble() * 300000 + 50000, 2);
                
                data.Add(new FuelTimeSeriesData
                {
                    Period = year.ToString(),
                    FuelCost = fuelCost
                });
            }
            
            return data;
        }

        public IActionResult TotalMaintenance()
        {
            ViewData["Title"] = "Total Maintenance Expenses";
            return View();
        }

        // GET: Finance/AddFinance
        public IActionResult AddFinance()
        {
            ViewData["Title"] = "Add Finance";
            
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View();
        }

        // POST: Finance/AddFinance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFinance(FinanceEntry financeEntry, IFormFile? attachment)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if provided
                if (attachment != null && attachment.Length > 0)
                {
                    // In a real application, you would save the file to storage
                    // For now, we'll just store the filename
                    financeEntry.AttachmentPath = attachment.FileName;
                }
                
                // In a real application, you would save to database here
                // For now, we'll just redirect back to FinancePage with success message
                TempData["Success"] = "Finance record added successfully!";
                return RedirectToAction(nameof(FinancePage));
            }
            
            // If validation fails, reload vehicles and return to view
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(financeEntry);
        }

        // GET: Finance/EditFinance/5
        public IActionResult EditFinance(int id)
        {
            ViewData["Title"] = "Edit Finance";
            
            // Get all finance entries (sample data matching FinancePage)
            var allFinanceEntries = new List<FinanceEntry>
            {
                new FinanceEntry { Id = 1001, VehicleId = 1, ExpenseType = "Fuel", Amount = 185.75m, ExpenseDate = new DateTime(2025, 12, 10), Description = "Regular fuel fill-up", ReferenceId = 1, AttachmentPath = "receipt_001.jpg" },
                new FinanceEntry { Id = 1002, VehicleId = 2, ExpenseType = "Maintenance", Amount = 450.00m, ExpenseDate = new DateTime(2025, 12, 08), Description = "Brake pad replacement", ReferenceId = 102, AttachmentPath = "maintenance_002.pdf" },
                new FinanceEntry { Id = 1003, VehicleId = 3, ExpenseType = "Other", Amount = 1200.00m, ExpenseDate = new DateTime(2025, 12, 01), Description = "Annual insurance premium", ReferenceId = null, AttachmentPath = "insurance_003.pdf" },
                new FinanceEntry { Id = 1004, VehicleId = 4, ExpenseType = "Fuel", Amount = 350.00m, ExpenseDate = new DateTime(2025, 12, 05), Description = "Premium gasoline", ReferenceId = 4, AttachmentPath = "fuel_004.jpg" },
                new FinanceEntry { Id = 1005, VehicleId = 5, ExpenseType = "Maintenance", Amount = 75.50m, ExpenseDate = new DateTime(2025, 12, 12), Description = "Oil change service", ReferenceId = 105, AttachmentPath = "oil_005.pdf" },
                new FinanceEntry { Id = 1006, VehicleId = 1, ExpenseType = "Other", Amount = 220.30m, ExpenseDate = new DateTime(2025, 12, 15), Description = "Toll fees and parking", ReferenceId = null, AttachmentPath = "toll_006.jpg" }
            };
            
            // Find the specific finance entry by ID
            var financeEntry = allFinanceEntries.FirstOrDefault(f => f.Id == id);
            
            // If not found, create a default entry
            if (financeEntry == null)
            {
                financeEntry = new FinanceEntry
                {
                    Id = id,
                    VehicleId = 1,
                    ExpenseType = "Fuel",
                    Amount = 0,
                    ExpenseDate = DateTime.Now,
                    Description = "Entry not found",
                    ReferenceId = null,
                    AttachmentPath = null
                };
            }
            
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(financeEntry);
        }

        // POST: Finance/EditFinance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFinance(FinanceEntry financeEntry, IFormFile? attachment)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload if provided
                if (attachment != null && attachment.Length > 0)
                {
                    // In a real application, you would save the file to storage
                    // For now, we'll just store the filename
                    financeEntry.AttachmentPath = attachment.FileName;
                }
                
                // In a real application, you would update the database here
                // For now, we'll just redirect back to FinancePage with success message
                TempData["Success"] = "Finance record updated successfully!";
                return RedirectToAction(nameof(FinancePage));
            }
            
            // If validation fails, reload vehicles and return to view
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(financeEntry);
        }

        // GET: Finance/ViewFinance/5
        public IActionResult ViewFinance(int id)
        {
            ViewData["Title"] = "View Finance";
            
            // Get all finance entries (same data as FinancePage and EditFinance)
            var allFinanceEntries = new List<FinanceEntry>
            {
                new FinanceEntry { Id = 1001, VehicleId = 1, ExpenseType = "Fuel", Amount = 185.75m, ExpenseDate = new DateTime(2025, 12, 10), Description = "Regular fuel fill-up", ReferenceId = 1, AttachmentPath = "receipt_001.jpg" },
                new FinanceEntry { Id = 1002, VehicleId = 2, ExpenseType = "Maintenance", Amount = 450.00m, ExpenseDate = new DateTime(2025, 12, 08), Description = "Brake pad replacement", ReferenceId = 102, AttachmentPath = "maintenance_002.pdf" },
                new FinanceEntry { Id = 1003, VehicleId = 3, ExpenseType = "Other", Amount = 1200.00m, ExpenseDate = new DateTime(2025, 12, 01), Description = "Annual insurance premium", ReferenceId = null, AttachmentPath = "insurance_003.pdf" },
                new FinanceEntry { Id = 1004, VehicleId = 4, ExpenseType = "Fuel", Amount = 350.00m, ExpenseDate = new DateTime(2025, 12, 05), Description = "Premium gasoline", ReferenceId = 4, AttachmentPath = "fuel_004.jpg" },
                new FinanceEntry { Id = 1005, VehicleId = 5, ExpenseType = "Maintenance", Amount = 75.50m, ExpenseDate = new DateTime(2025, 12, 12), Description = "Oil change service", ReferenceId = 105, AttachmentPath = "oil_005.pdf" },
                new FinanceEntry { Id = 1006, VehicleId = 1, ExpenseType = "Other", Amount = 220.30m, ExpenseDate = new DateTime(2025, 12, 15), Description = "Toll fees and parking", ReferenceId = null, AttachmentPath = "toll_006.jpg" }
            };
            
            // Find the specific finance entry by ID
            var financeEntry = allFinanceEntries.FirstOrDefault(f => f.Id == id);
            
            // If not found, create a default entry
            if (financeEntry == null)
            {
                financeEntry = new FinanceEntry
                {
                    Id = id,
                    VehicleId = 1,
                    ExpenseType = "Fuel",
                    Amount = 0,
                    ExpenseDate = DateTime.Now,
                    Description = "Entry not found",
                    ReferenceId = null,
                    AttachmentPath = null
                };
            }
            
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(financeEntry);
        }
    }
}
