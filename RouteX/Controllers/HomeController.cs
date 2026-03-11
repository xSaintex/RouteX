using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using RouteX.ViewModels;
using System.Globalization;

namespace RouteX.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IAuditService auditService)
        {
            _context = context;
            _userManager = userManager;
            _auditService = auditService;
            _logger = logger;
        }

        private static string FormatPhilippinePeso(decimal amount)
        {
            return amount.ToString("C", new CultureInfo("en-PH"));
        }

        private async Task<(bool IsSuperAdmin, int? BranchId)> GetBranchContextAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            return (isSuperAdmin, user?.BranchId);
        }

        public async Task<IActionResult> Index()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            // Check if user is Admin (but not SuperAdmin)
            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                // Admins see their branch data only, hide recent activities
                var adminDashboardData = await GetDashboardDataAsync();
                adminDashboardData.RecentActivities = new List<RecentActivityItem>(); // Hide recent activities for Admins
                return View("Index", adminDashboardData);
            }

            // SuperAdmin sees all data
            var dashboardData = await GetDashboardDataAsync();
            return View(dashboardData);
        }

        private async Task<DashboardViewModel> GetDashboardDataAsync()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();

            // Get vehicles
            var vehiclesQuery = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived);

            // Get maintenance (completed only for expense calculation)
            var maintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => (m.IsArchived == null || m.IsArchived == false) && m.Status == (int)MaintenanceStatus.Completed);

            // Get finance
            var financeQuery = _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived);

            // Get trips (completed trips only for TotalTrips count)
            var tripsQuery = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed);

            // Filter by branch for non-SuperAdmin users
            if (!isSuperAdmin && branchId.HasValue)
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.BranchId == branchId.Value);
                maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == branchId.Value);
                financeQuery = financeQuery.Where(f => f.BranchId == branchId.Value);
                tripsQuery = tripsQuery.Where(r => r.BranchId == branchId.Value);
            }

            var vehicles = await vehiclesQuery.ToListAsync();
            var maintenanceEntries = await maintenanceQuery.ToListAsync();
            var financeEntries = await financeQuery.ToListAsync();
            var trips = await tripsQuery.ToListAsync();

            // Calculate total vehicles
            var totalVehicles = vehicles.Count;

            // Calculate active vehicles
            var activeVehicles = vehicles.Count(v => v.Status == VehicleStatus.Active);

            // Calculate pending maintenance (separate query since main query is for completed maintenance only)
            var pendingMaintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => m.IsArchived != true);

            if (!isSuperAdmin && branchId.HasValue)
            {
                pendingMaintenanceQuery = pendingMaintenanceQuery.Where(m => m.BranchId == branchId.Value);
            }

            var pendingMaintenance = (await pendingMaintenanceQuery.CountAsync(m => (m.Status ?? 0) == 0));

            // Get fuel entries for fuel cost calculation
            var fuelQuery = _context.FuelEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived);

            // Filter by branch for non-SuperAdmin users
            if (!isSuperAdmin && branchId.HasValue)
            {
                fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var fuelEntries = await fuelQuery.ToListAsync();

            // Calculate total expenses (including finance, maintenance, and fuel)
            var totalExpenses = financeEntries.Sum(f => f.Amount) + maintenanceEntries.Sum(m => m.Cost ?? 0) + fuelEntries.Sum(f => f.TotalCost);

            // Calculate total trips
            var totalTrips = trips.Count;

            var totalFuelCost = fuelEntries.Sum(f => f.TotalCost);

            return new DashboardViewModel
            {
                FuelConsumptionData = GetFuelConsumptionData(),
                VehicleStatusData = GetVehicleStatusData(),
                MostActiveVehicles = GetMostActiveVehicles(),
                TotalVehicles = totalVehicles,
                ActiveVehicles = activeVehicles,
                PendingMaintenance = pendingMaintenance,
                TotalExpenses = totalExpenses,
                TotalTrips = totalTrips,
                RecentActivities = await GetRecentActivitiesAsync(),
                TotalFuelCost = totalFuelCost
            };
        }

        private async Task<List<RecentActivityItem>> GetRecentActivitiesAsync()
        {
            var activities = await _context.AuditLogs
                .AsNoTracking()
                .Where(a => a.UserId != "SYSTEM")
                .OrderByDescending(a => a.ActionDate)
                .Take(10)
                .Select(a => new RecentActivityItem
                {
                    Action = a.Action,
                    UserId = a.UserId,
                    ActionDate = a.ActionDate
                })
                .ToListAsync();

            return activities;
        }

        private List<FuelConsumptionPoint> GetFuelConsumptionData()
        {
            var (isSuperAdmin, branchId) = GetBranchContextAsync().GetAwaiter().GetResult();
            var startDate = DateTime.Today.AddDays(-29); // Last 30 days
            var endDate = DateTime.Today.AddDays(1);

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[FuelConsumption] isSuperAdmin: {isSuperAdmin}, branchId: {branchId}");
            System.Diagnostics.Debug.WriteLine($"[FuelConsumption] Date range: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}");

            // Get fuel data from FinanceEntries (ExpenseType = Fuel)
            var financeFuelQuery = _context.FinanceEntries
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Where(f => !f.IsArchived 
                    && f.ExpenseDate >= startDate 
                    && f.ExpenseDate < endDate
                    && (f.ExpenseType == "Fuel" || f.ExpenseType == "FUEL" || f.ExpenseType == "fuel"));

            // Get fuel data from FuelEntries
            var fuelEntriesQuery = _context.FuelEntries
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Where(f => !f.IsArchived && f.DateTime >= startDate && f.DateTime < endDate);

            // Filter by branch for non-SuperAdmin users
            if (!isSuperAdmin && branchId.HasValue)
            {
                financeFuelQuery = financeFuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
                fuelEntriesQuery = fuelEntriesQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var financeFuelEntries = financeFuelQuery.ToList();
            var fuelEntries = fuelEntriesQuery.ToList();

            // Debug logging
            System.Diagnostics.Debug.WriteLine($"[FuelConsumption] FinanceEntries count: {financeFuelEntries.Count}");
            System.Diagnostics.Debug.WriteLine($"[FuelConsumption] FuelEntries count: {fuelEntries.Count}");

            // Calculate distance traveled and total fuel spend by date
            var distanceByDate = new Dictionary<DateTime, decimal>();
            var spendByDate = new Dictionary<DateTime, decimal>();

            // Process FinanceEntries fuel data (use Vehicle's odometer if available)
            foreach (var entry in financeFuelEntries)
            {
                var date = entry.ExpenseDate.Date;
                if (!spendByDate.ContainsKey(date))
                    spendByDate[date] = 0;
                spendByDate[date] += entry.Amount;
            }

            // Process FuelEntries data with odometer for distance calculation
            var vehicleIds = fuelEntries.Select(e => e.VehicleId).Distinct().ToList();
            
            // Get last odometer reading before the date range for each vehicle
            var previousOdometers = new Dictionary<int, decimal>();
            foreach (var vid in vehicleIds)
            {
                var lastEntryBefore = _context.FuelEntries
                    .AsNoTracking()
                    .Where(f => f.VehicleId == vid && f.DateTime < startDate && !f.IsArchived)
                    .OrderByDescending(f => f.DateTime)
                    .Select(f => (decimal?)f.Odometer)
                    .FirstOrDefault();
                
                previousOdometers[vid] = lastEntryBefore ?? 0;
            }
            
            var vehicleGroups = fuelEntries.GroupBy(e => e.VehicleId).ToList();
            foreach (var vehicleGroup in vehicleGroups)
            {
                var vehicleId = vehicleGroup.Key;
                var vehicleEntries = vehicleGroup.OrderBy(e => e.DateTime).ToList();
                decimal previousOdometer = previousOdometers.ContainsKey(vehicleId) ? previousOdometers[vehicleId] : 0;
                
                foreach (var entry in vehicleEntries)
                {
                    var date = entry.DateTime.Date;
                    var currentOdometer = (decimal)entry.Odometer;
                    var distance = previousOdometer > 0 && currentOdometer > previousOdometer 
                        ? currentOdometer - previousOdometer 
                        : 0;
                    
                    if (!distanceByDate.ContainsKey(date))
                        distanceByDate[date] = 0;
                    if (!spendByDate.ContainsKey(date))
                        spendByDate[date] = 0;
                        
                    distanceByDate[date] += distance;
                    spendByDate[date] += entry.TotalCost;
                    
                    previousOdometer = currentOdometer;
                }
            }

            var data = new List<FuelConsumptionPoint>();
            for (var i = 0; i < 30; i++) // 30 days of data points
            {
                var date = startDate.AddDays(i);
                
                data.Add(new FuelConsumptionPoint
                {
                    Date = date.ToString("MMM dd"),
                    TotalFuelSpend = (double)(spendByDate.ContainsKey(date) ? spendByDate[date] : 0m),
                    DistanceTraveled = distanceByDate.ContainsKey(date) ? distanceByDate[date] : 0m
                });
            }
            return data;
        }

        private List<VehicleStatusData> GetVehicleStatusData()
        {
            var (isSuperAdmin, branchId) = GetBranchContextAsync().GetAwaiter().GetResult();

            var vehiclesQuery = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .Where(v => isSuperAdmin || v.BranchId == branchId);

            var vehicles = vehiclesQuery.ToList();

            var totalVehicles = vehicles.Count;
            var activeCount = vehicles.Count(v => v.Status == VehicleStatus.Active);
            var maintenanceCount = vehicles.Count(v => v.Status == VehicleStatus.Maintenance);
            var inactiveCount = vehicles.Count(v => v.Status == VehicleStatus.Inactive);

            return new List<VehicleStatusData>
            {
                new VehicleStatusData { Status = "Active", Count = activeCount, Percentage = totalVehicles > 0 ? (int)Math.Round((double)activeCount / totalVehicles * 100) : 0 },
                new VehicleStatusData { Status = "Maintenance", Count = maintenanceCount, Percentage = totalVehicles > 0 ? (int)Math.Round((double)maintenanceCount / totalVehicles * 100) : 0 },
                new VehicleStatusData { Status = "Inactive", Count = inactiveCount, Percentage = totalVehicles > 0 ? (int)Math.Round((double)inactiveCount / totalVehicles * 100) : 0 }
            };
        }

        private List<ActiveVehicleData> GetMostActiveVehicles()
        {
            var (isSuperAdmin, branchId) = GetBranchContextAsync().GetAwaiter().GetResult();

            var mileageByVehicleId = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed) // Match VehiclesController logic
                .Where(r => isSuperAdmin || r.BranchId == null || r.BranchId == branchId)
                .GroupBy(r => r.VehicleId)
                .Select(g => new { VehicleId = g.Key, TotalKm = g.Sum(r => r.DistanceKm) })
                .ToList()
                .ToDictionary(g => g.VehicleId, g => g.TotalKm);

            var vehicles = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .Where(v => isSuperAdmin || v.BranchId == branchId)
                .ToList();

            var topVehicles = vehicles
                .Select(v => new
                {
                    Vehicle = v,
                    Mileage = mileageByVehicleId.TryGetValue(v.Id, out var km) ? km : 0m
                })
                .OrderByDescending(x => x.Mileage)
                .Take(5)
                .Select((x, index) => new ActiveVehicleData
                {
                    Rank = index + 1,
                    PlateNumber = x.Vehicle.PlateNumber,
                    UnitModel = x.Vehicle.UnitModel,
                    DistanceKm = (int)(x.Mileage ?? 0m)
                })
                .ToList();

            return topVehicles;
        }

        public async Task<IActionResult> FinanceDashboard()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            // Get finance-specific data
            var financeViewModel = await GetFinanceDashboardDataAsync();
            return View("FinanceDashboard", financeViewModel);
        }

        public async Task<IActionResult> OpStaffDashboard()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;

            // Get operations-specific data
            var opsViewModel = await GetOpStaffDashboardDataAsync();
            return View("OpStaffDashboard", opsViewModel);
        }

        private async Task<FinanceDashboardViewModel> GetFinanceDashboardDataAsync()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();

            // Get finance entries
            var financeQuery = _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived);

            // Get maintenance entries
            var maintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => m.IsArchived != true);

            // Get fuel entries
            var fuelQuery = _context.FuelEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived);

            // Filter by branch for non-SuperAdmin users
            if (!isSuperAdmin && branchId.HasValue)
            {
                financeQuery = financeQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
                maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == null || m.BranchId == branchId.Value);
                fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var financeEntries = await financeQuery.ToListAsync();
            var maintenanceEntries = await maintenanceQuery.ToListAsync();
            var fuelEntries = await fuelQuery.ToListAsync();

            // Create recent expenses
            var recentExpenses = new List<RecentExpenseItem>();
            
            // Add finance entries
            recentExpenses.AddRange(financeEntries.Select(f => new RecentExpenseItem
            {
                Category = "Finance",
                Vehicle = f.Vehicle?.UnitModel ?? "Unknown",
                Amount = f.Amount,
                Date = f.ExpenseDate,
                Description = f.Description
            }));

            // Add maintenance entries
            recentExpenses.AddRange(maintenanceEntries.Select(m => new RecentExpenseItem
            {
                Category = "Maintenance",
                Vehicle = $"{m.UnitModel} ({m.PlateNumber})",
                Amount = m.Cost ?? 0,
                Date = m.Date ?? DateTime.MinValue,
                Description = m.Description
            }));

            // Add fuel entries
            recentExpenses.AddRange(fuelEntries.Select(f => new RecentExpenseItem
            {
                Category = "Fuel",
                Vehicle = $"{f.UnitModel} ({f.PlateNumber})",
                Amount = f.TotalCost,
                Date = f.Date,
                Description = $"{f.Liters}L at {f.FuelStation}"
            }));

            // Sort by date and take recent ones
            var recentItems = recentExpenses
                .OrderByDescending(e => e.Date)
                .Take(10)
                .ToList();

            // Create cost comparison
            var totalFuelCost = fuelEntries.Sum(f => f.TotalCost);
            var totalMaintenanceCost = maintenanceEntries.Sum(m => m.Cost ?? 0);
            var totalFinanceCost = financeEntries.Sum(f => f.Amount);

            var costComparison = new List<CostComparisonItem>
            {
                new CostComparisonItem { Category = "Fuel", Amount = totalFuelCost },
                new CostComparisonItem { Category = "Maintenance", Amount = totalMaintenanceCost },
                new CostComparisonItem { Category = "Other Expenses", Amount = totalFinanceCost }
            };

            return new FinanceDashboardViewModel
            {
                FuelConsumptionData = GetFuelConsumptionData(),
                RecentExpenses = recentItems,
                CostComparison = costComparison
            };
        }

        private async Task<OpStaffDashboardViewModel> GetOpStaffDashboardDataAsync()
        {
            var (isSuperAdmin, branchId) = await GetBranchContextAsync();

            // Get vehicles
            var vehiclesQuery = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived);

            // Get trips (completed trips only, not just all trip history)
            var tripsQuery = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed);

            // Filter by branch for non-SuperAdmin users
            if (!isSuperAdmin && branchId.HasValue)
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.BranchId == branchId.Value);
                tripsQuery = tripsQuery.Where(r => r.BranchId == branchId.Value);
            }

            var vehicles = await vehiclesQuery.ToListAsync();
            var trips = await tripsQuery.ToListAsync();

            // Categorize vehicles correctly
            var activeVehicles = vehicles.Where(v => v.Status == VehicleStatus.Active).ToList();
            var inactiveVehicles = vehicles.Where(v => v.Status == VehicleStatus.Inactive).ToList();
            var maintenanceVehicles = vehicles.Where(v => v.Status == VehicleStatus.Maintenance).ToList();

            // Get most active vehicles by total mileage
            var mostActiveVehicles = GetMostActiveVehiclesForBranch(isSuperAdmin, branchId);

            // Get total trips count
            var totalTrips = trips.Count;

            return new OpStaffDashboardViewModel
            {
                ActiveVehicles = activeVehicles,
                InactiveVehicles = inactiveVehicles,
                MaintenanceVehicles = maintenanceVehicles,
                VehicleStatusData = GetVehicleStatusDataForBranch(isSuperAdmin, branchId),
                MostActiveVehicles = mostActiveVehicles,
                TotalTrips = totalTrips
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        private List<ActiveVehicleData> GetMostActiveVehiclesForBranch(bool isSuperAdmin, int? branchId)
        {
            var mileageByVehicleId = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed) // Match VehiclesController logic
                .Where(r => isSuperAdmin || r.BranchId == null || r.BranchId == branchId)
                .GroupBy(r => r.VehicleId)
                .Select(g => new { VehicleId = g.Key, TotalKm = g.Sum(r => r.DistanceKm) })
                .ToList()
                .ToDictionary(g => g.VehicleId, g => g.TotalKm);

            var vehicles = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .Where(v => isSuperAdmin || v.BranchId == branchId)
                .ToList();

            var topVehicles = vehicles
                .Select(v => new
                {
                    Vehicle = v,
                    Mileage = mileageByVehicleId.TryGetValue(v.Id, out var km) ? km : 0m
                })
                .OrderByDescending(x => x.Mileage)
                .Take(5)
                .Select((x, index) => new ActiveVehicleData
                {
                    Rank = index + 1,
                    PlateNumber = x.Vehicle.PlateNumber,
                    UnitModel = x.Vehicle.UnitModel,
                    DistanceKm = (int)(x.Mileage ?? 0m)
                })
                .ToList();

            return topVehicles;
        }

        private List<VehicleStatusData> GetVehicleStatusDataForBranch(bool isSuperAdmin, int? branchId)
        {
            var vehiclesQuery = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .Where(v => isSuperAdmin || v.BranchId == branchId);

            var vehicles = vehiclesQuery.ToList();
            
            var statusData = new List<VehicleStatusData>
            {
                new VehicleStatusData { Status = "Active", Count = vehicles.Count(v => v.Status == VehicleStatus.Active) },
                new VehicleStatusData { Status = "Inactive", Count = vehicles.Count(v => v.Status == VehicleStatus.Inactive) },
                new VehicleStatusData { Status = "Maintenance", Count = vehicles.Count(v => v.Status == VehicleStatus.Maintenance) }
            };

            return statusData;
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
