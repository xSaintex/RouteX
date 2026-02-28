using System.Diagnostics;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;
using RouteX.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace RouteX.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var dashboardData = await GetDashboardDataAsync();
            return View(dashboardData);
        }

        public async Task<IActionResult> FinanceDashboard()
        {
            ViewData["Title"] = "Finance Dashboard";

            var recentFuel = await _context.FuelEntries
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Where(f => !f.IsArchived)
                .OrderByDescending(f => f.DateTime)
                .Take(10)
                .Select(f => new RecentExpenseItem
                {
                    Category = "Fuel",
                    Vehicle = f.Vehicle != null ? $"{f.Vehicle.PlateNumber} - {f.Vehicle.UnitModel}" : $"{f.PlateNumber} - {f.UnitModel}",
                    Amount = f.TotalCost,
                    Date = f.DateTime,
                    Description = f.Notes
                })
                .ToListAsync();

            var recentMaintenance = await _context.MaintenanceEntries
                .AsNoTracking()
                .Include(m => m.Vehicle)
                .Where(m => m.IsArchived != true && m.Status == (int)MaintenanceStatus.Completed)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .Select(m => new RecentExpenseItem
                {
                    Category = "Maintenance",
                    Vehicle = m.Vehicle != null ? $"{m.Vehicle.PlateNumber} - {m.Vehicle.UnitModel}" : (m.PlateNumber ?? "Unknown"),
                    Amount = m.Cost ?? 0,
                    Date = m.Date ?? DateTime.MinValue,
                    Description = m.Description
                })
                .ToListAsync();

            var recentExpenses = recentFuel
                .Concat(recentMaintenance)
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToList();

            var financeEntries = await _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived)
                .ToListAsync();

            var fuelTotal = financeEntries
                .Where(f => string.Equals(f.ExpenseType, "Fuel", StringComparison.OrdinalIgnoreCase))
                .Sum(f => f.Amount);
            var maintenanceTotal = financeEntries
                .Where(f => string.Equals(f.ExpenseType, "Maintenance", StringComparison.OrdinalIgnoreCase))
                .Sum(f => f.Amount);
            var otherTotal = financeEntries
                .Where(f => !string.Equals(f.ExpenseType, "Fuel", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(f.ExpenseType, "Maintenance", StringComparison.OrdinalIgnoreCase))
                .Sum(f => f.Amount);

            var viewModel = new FinanceDashboardViewModel
            {
                FuelConsumptionData = GetFuelConsumptionData(),
                RecentExpenses = recentExpenses,
                CostComparison = new List<CostComparisonItem>
                {
                    new() { Category = "Fuel", Amount = fuelTotal },
                    new() { Category = "Maintenance", Amount = maintenanceTotal },
                    new() { Category = "Other", Amount = otherTotal }
                }
            };

            return View(viewModel);
        }

        public async Task<IActionResult> OpStaffDashboard()
        {
            ViewData["Title"] = "Operations Dashboard";

            var vehicles = await _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .ToListAsync();

            var activeVehicles = vehicles
                .Where(v => v.Status == VehicleStatus.Active)
                .OrderBy(v => v.PlateNumber)
                .ToList();

            var idleVehicles = vehicles
                .Where(v => v.Status == VehicleStatus.Inactive)
                .OrderBy(v => v.PlateNumber)
                .ToList();

            var statusData = new List<VehicleStatusData>();
            var totalVehicles = vehicles.Count;
            if (totalVehicles > 0)
            {
                var activeCount = vehicles.Count(v => v.Status == VehicleStatus.Active);
                var maintenanceCount = vehicles.Count(v => v.Status == VehicleStatus.Maintenance);
                var inactiveCount = vehicles.Count(v => v.Status == VehicleStatus.Inactive);

                statusData.Add(new VehicleStatusData
                {
                    Status = "Active",
                    Count = activeCount,
                    Percentage = (int)Math.Round((double)activeCount / totalVehicles * 100)
                });
                statusData.Add(new VehicleStatusData
                {
                    Status = "Maintenance",
                    Count = maintenanceCount,
                    Percentage = (int)Math.Round((double)maintenanceCount / totalVehicles * 100)
                });
                statusData.Add(new VehicleStatusData
                {
                    Status = "Inactive",
                    Count = inactiveCount,
                    Percentage = (int)Math.Round((double)inactiveCount / totalVehicles * 100)
                });
            }

            var maintenanceVehicles = vehicles
                .Where(v => v.Status == VehicleStatus.Maintenance)
                .OrderBy(v => v.PlateNumber)
                .ToList();

            var viewModel = new OpStaffDashboardViewModel
            {
                ActiveVehicles = activeVehicles,
                IdleVehicles = idleVehicles,
                VehicleStatusData = statusData,
                MaintenanceVehicles = maintenanceVehicles,
                ActiveTripsToday = activeVehicles
                    .OrderByDescending(v => v.Mileage)
                    .Take(5)
                    .ToList()
            };

            return View(viewModel);
        }

        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var vehiclesQuery = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived);

            var totalVehicles = await vehiclesQuery.CountAsync();
            var activeVehicles = await vehiclesQuery.CountAsync(v => v.Status == VehicleStatus.Active);

            var pendingMaintenance = await _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => m.IsArchived != true && (m.Status ?? (int)MaintenanceStatus.Pending) == (int)MaintenanceStatus.Pending)
                .CountAsync();

            var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var startMonth = currentMonthStart.AddMonths(-23);
            var endMonthExclusive = currentMonthStart.AddMonths(1);
            var totalExpenses = await _context.FinanceEntries
                .AsNoTracking()
                .Where(f => f.ExpenseDate >= startMonth && f.ExpenseDate < endMonthExclusive)
                .SumAsync(f => f.Amount);

            return new DashboardViewModel
            {
                FuelConsumptionData = GetFuelConsumptionData(),
                VehicleStatusData = GetVehicleStatusData(),
                MostActiveVehicles = GetMostActiveVehicles(),
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                PendingMaintenance = pendingMaintenance,
                TotalExpenses = totalExpenses
            };
        }

        private List<FuelConsumptionPoint> GetFuelConsumptionData()
        {
            var startDate = DateTime.Today.AddDays(-29);
            var endDate = DateTime.Today.AddDays(1);

            var entries = _context.FuelEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived && f.DateTime >= startDate && f.DateTime < endDate)
                .Select(f => new { Date = f.DateTime.Date, f.TotalCost, f.Odometer })
                .ToList();

            var totalsByDate = entries
                .GroupBy(e => e.Date)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        TotalSpend = g.Sum(x => x.TotalCost),
                        Distance = g.Sum(x => x.Odometer)
                    });

            var data = new List<FuelConsumptionPoint>();
            for (var i = 0; i < 30; i++)
            {
                var date = startDate.AddDays(i);
                totalsByDate.TryGetValue(date, out var totals);

                data.Add(new FuelConsumptionPoint
                {
                    Date = date.ToString("MMM dd"),
                    TotalFuelSpend = (double)(totals?.TotalSpend ?? 0),
                    DistanceTraveled = totals?.Distance ?? 0
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

    public class DashboardViewModel
    {
        public List<FuelConsumptionPoint> FuelConsumptionData { get; set; } = null!;
        public List<VehicleStatusData> VehicleStatusData { get; set; } = null!;
        public List<ActiveVehicleData> MostActiveVehicles { get; set; } = null!;
        public int TotalVehicles { get; set; }
        public int ActiveVehicles { get; set; }
        public int PendingMaintenance { get; set; }
        public decimal TotalExpenses { get; set; }
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
