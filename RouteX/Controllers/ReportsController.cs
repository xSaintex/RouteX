using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;

namespace RouteX.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IAuditService _auditService;
        private readonly ApplicationDbContext _context;

        public ReportsController(IAuditService auditService, ApplicationDbContext context)
        {
            _auditService = auditService;
            _context = context;
        }

        public async Task<IActionResult> ReportsPage()
        {
            ViewData["Title"] = "Reports";

            var (isSuperAdmin, branchId) = await GetBranchContextAsync();

            var reportTypes = new List<string> { "Expenses", "Fuel", "Maintenance", "Vehicle" };
            var summary = await GetSummaryAsync(isSuperAdmin, branchId);
            var monthlyBreakdown = await GetMonthlyExpenseBreakdownAsync(isSuperAdmin, branchId);
            var monthlyBudgets = await GetMonthlyBudgetsAsync(isSuperAdmin, branchId);
            var reportRecords = await GetReportRecordsAsync("Expenses", DateTime.Today.AddDays(-30), DateTime.Today, isSuperAdmin, branchId);

            var viewModel = new ReportsViewModel
            {
                ReportTypes = reportTypes,
                SelectedReportType = "Expenses",
                SelectedDateRange = "Last 30 Days",
                TotalExpenses = summary.TotalExpenses,
                TotalFuelCost = summary.TotalFuelCost,
                TotalMaintenanceCost = summary.TotalMaintenanceCost,
                TotalTrips = summary.TotalTrips,
                ReportRecords = reportRecords,
                MonthlyExpenseBreakdown = monthlyBreakdown,
                MonthlyBudget = 100000m,
                MonthlyBudgets = monthlyBudgets
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogExport(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return BadRequest();
            }

            var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
            await _auditService.LogActionAsync(actingUser, $"Export:Reports:{format}");
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ReportData(string reportType, string dateRange, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrWhiteSpace(reportType))
            {
                return BadRequest();
            }

            var (isSuperAdmin, branchId) = await GetBranchContextAsync();
            var (rangeStart, rangeEnd) = ResolveDateRange(dateRange, startDate, endDate);
            
            if (reportType == "Expenses")
            {
                // Get expense records like FinancePage
                var expenseRecords = await GetExpenseRecordsAsync(rangeStart, rangeEnd, isSuperAdmin, branchId);
                return Json(expenseRecords.Select(r => new
                {
                    r.ExpenseId,
                    r.Vehicle,
                    r.ExpenseType,
                    r.Amount,
                    r.ExpenseDate,
                    r.Description,
                    r.Branch
                }));
            }
            else
            {
                // Get other report types as before
                var records = await GetReportRecordsAsync(reportType, rangeStart, rangeEnd, isSuperAdmin, branchId);
                return Json(records.Select(r => new
                {
                    r.Id,
                    r.Vehicle,
                    r.Category,
                    r.Amount,
                    Date = r.Date == DateTime.MinValue ? string.Empty : r.Date.ToString("yyyy-MM-dd"),
                    r.Description
                }));
            }
        }

        private async Task<List<ExpenseRecord>> GetExpenseRecordsAsync(DateTime startDate, DateTime endDate, bool isSuperAdmin, int? branchId)
        {
            var endExclusive = endDate.AddDays(1);

            // Get finance entries
            var financeQuery = _context.FinanceEntries
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Include(f => f.Branch)
                .Where(f => !f.IsArchived && f.ExpenseDate >= startDate && f.ExpenseDate < endExclusive);

            if (!isSuperAdmin && branchId.HasValue)
            {
                financeQuery = financeQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var financeEntries = await financeQuery.ToListAsync();

            // Get fuel entries
            var fuelQuery = _context.FuelEntries
                .AsNoTracking()
                .Include(f => f.Vehicle)
                .Include(f => f.Branch)
                .Where(f => !f.IsArchived && f.DateTime >= startDate && f.DateTime < endExclusive);

            if (!isSuperAdmin && branchId.HasValue)
            {
                fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var fuelEntries = await fuelQuery.ToListAsync();

            // Get completed maintenance entries
            var maintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Include(m => m.Vehicle)
                .Include(m => m.Branch)
                .Where(m => (m.IsArchived == null || m.IsArchived == false) && m.Status == 2 && m.Date >= startDate && m.Date < endExclusive);

            if (!isSuperAdmin && branchId.HasValue)
            {
                maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == null || m.BranchId == branchId.Value);
            }

            var maintenanceEntries = await maintenanceQuery.ToListAsync();

            // Combine all expense records like FinanceController
            var allExpenseRecords = new List<ExpenseRecord>();

            allExpenseRecords.AddRange(financeEntries.Select(f => new ExpenseRecord
            {
                ExpenseId = f.Id,
                Vehicle = $"{f.Vehicle?.PlateNumber} - {f.Vehicle?.UnitModel}",
                ExpenseType = f.ExpenseType,
                Amount = f.Amount,
                ExpenseDate = f.ExpenseDate,
                Description = f.Description ?? "No description",
                CreatedDate = f.ExpenseDate,
                Branch = f.Branch
            }));

            allExpenseRecords.AddRange(fuelEntries.Select(f => new ExpenseRecord
            {
                ExpenseId = f.Id,
                Vehicle = $"{f.Vehicle?.PlateNumber} - {f.Vehicle?.UnitModel}",
                ExpenseType = "FUEL",
                Amount = f.TotalCost,
                ExpenseDate = f.DateTime,
                Description = f.Notes ?? "Fuel purchase",
                CreatedDate = f.DateTime,
                Branch = f.Branch
            }));

            allExpenseRecords.AddRange(maintenanceEntries.Select(m => new ExpenseRecord
            {
                ExpenseId = m.Id,
                Vehicle = $"{m.Vehicle?.PlateNumber} - {m.Vehicle?.UnitModel}",
                ExpenseType = "MAINTENANCE",
                Amount = m.Cost ?? 0,
                ExpenseDate = m.Date ?? default,
                Description = m.Description ?? "Maintenance service",
                CreatedDate = m.Date ?? default,
                Branch = m.Branch
            }));

            return allExpenseRecords.OrderByDescending(e => e.ExpenseDate).ToList();
        }

        private static (DateTime Start, DateTime End) ResolveDateRange(string dateRange, DateTime? startDate, DateTime? endDate)
        {
            var today = DateTime.Today;

            return dateRange switch
            {
                "Last 7 Days" => (today.AddDays(-6), today),
                "Last 30 Days" => (today.AddDays(-29), today),
                "Last 3 Months" => (today.AddMonths(-3).AddDays(1 - today.Day), today),
                "Last 6 Months" => (today.AddMonths(-6).AddDays(1 - today.Day), today),
                "Last Year" => (today.AddYears(-1).AddDays(1 - today.Day), today),
                "Custom Range" when startDate.HasValue && endDate.HasValue => (startDate.Value.Date, endDate.Value.Date),
                _ => (today.AddDays(-29), today)
            };
        }

        private async Task<(decimal TotalExpenses, decimal TotalFuelCost, decimal TotalMaintenanceCost, int TotalTrips)> GetSummaryAsync(bool isSuperAdmin, int? branchId)
        {
            var financeQuery = _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                financeQuery = financeQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var financeEntries = await financeQuery.ToListAsync();

            var fuelQuery = _context.FuelEntries
                .AsNoTracking()
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var fuelEntries = await fuelQuery.ToListAsync();

            var maintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => (m.IsArchived == null || m.IsArchived == false) && m.Status == (int)MaintenanceStatus.Completed)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == null || m.BranchId == branchId.Value);
            }

            var maintenanceEntries = await maintenanceQuery.ToListAsync();

            var totalExpenses = financeEntries.Sum(f => f.Amount) + maintenanceEntries.Sum(m => m.Cost ?? 0) + fuelEntries.Sum(f => f.TotalCost);
            var totalFuel = fuelEntries.Sum(f => f.TotalCost);
            var totalMaintenance = maintenanceEntries.Sum(m => m.Cost ?? 0);

            var tripsQuery = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                tripsQuery = tripsQuery.Where(r => r.BranchId == branchId.Value);
            }

            var totalTrips = await tripsQuery.CountAsync();

            return (totalExpenses, totalFuel, totalMaintenance, totalTrips);
        }

        private async Task<List<ReportRecord>> GetReportRecordsAsync(string reportType, DateTime startDate, DateTime endDate, bool isSuperAdmin, int? branchId)
        {
            var endExclusive = endDate.AddDays(1);

            switch (reportType)
            {
                case "Fuel":
                    var fuelQuery = _context.FuelEntries
                        .AsNoTracking()
                        .Include(f => f.Vehicle)
                        .Where(f => !f.IsArchived && f.DateTime >= startDate && f.DateTime < endExclusive)
                        .AsQueryable();

                    if (!isSuperAdmin && branchId.HasValue)
                    {
                        fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
                    }

                    return await fuelQuery
                        .OrderByDescending(f => f.DateTime)
                        .Select(f => new ReportRecord
                        {
                            Id = f.Id,
                            Vehicle = f.Vehicle != null ? $"{f.Vehicle.PlateNumber} - {f.Vehicle.UnitModel}" : $"{f.PlateNumber} - {f.UnitModel}",
                            Category = "Fuel",
                            Amount = f.TotalCost,
                            Date = f.DateTime,
                            Description = f.Notes
                        })
                        .ToListAsync();
                case "Maintenance":
                    var maintenanceQuery = _context.MaintenanceEntries
                        .AsNoTracking()
                        .Include(m => m.Vehicle)
                        .Where(m => m.IsArchived != true && m.Date.HasValue && m.Date >= startDate && m.Date < endExclusive)
                        .AsQueryable();

                    if (!isSuperAdmin && branchId.HasValue)
                    {
                        maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == null || m.BranchId == branchId.Value);
                    }

                    return await maintenanceQuery
                        .OrderByDescending(m => m.Date)
                        .Select(m => new ReportRecord
                        {
                            Id = m.Id,
                            Vehicle = m.Vehicle != null ? $"{m.Vehicle.PlateNumber} - {m.Vehicle.UnitModel}" : (m.PlateNumber ?? "Unknown"),
                            Category = "Maintenance",
                            Amount = m.Cost ?? 0,
                            Date = m.Date ?? DateTime.MinValue,
                            Description = m.Description ?? string.Empty
                        })
                        .ToListAsync();
                case "Vehicle":
                    var vehicleQuery = _context.Vehicles
                        .AsNoTracking()
                        .Where(v => !v.IsArchived)
                        .AsQueryable();

                    if (!isSuperAdmin && branchId.HasValue)
                    {
                        vehicleQuery = vehicleQuery.Where(v => v.BranchId == null || v.BranchId == branchId.Value);
                    }

                    return await vehicleQuery
                        .OrderByDescending(v => v.Id)
                        .Select(v => new ReportRecord
                        {
                            Id = v.Id,
                            Vehicle = $"{v.PlateNumber} - {v.UnitModel}",
                            Category = v.Status.ToString(),
                            Amount = 0,
                            Date = DateTime.MinValue,
                            Description = v.VehicleType
                        })
                        .ToListAsync();
                default:
                    var financeQuery = _context.FinanceEntries
                        .AsNoTracking()
                        .Include(f => f.Vehicle)
                        .Where(f => !f.IsArchived && f.ExpenseDate >= startDate && f.ExpenseDate < endExclusive)
                        .AsQueryable();

                    if (!isSuperAdmin && branchId.HasValue)
                    {
                        financeQuery = financeQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
                    }

                    return await financeQuery
                        .OrderByDescending(f => f.ExpenseDate)
                        .Select(f => new ReportRecord
                        {
                            Id = f.Id,
                            Vehicle = f.Vehicle != null ? $"{f.Vehicle.PlateNumber} - {f.Vehicle.UnitModel}" : "Unknown",
                            Category = f.ExpenseType,
                            Amount = f.Amount,
                            Date = f.ExpenseDate,
                            Description = f.Description ?? string.Empty
                        })
                        .ToListAsync();
            }
        }

        private async Task<List<MonthlyExpenseBreakdownData>> GetMonthlyExpenseBreakdownAsync(bool isSuperAdmin, int? branchId)
        {
            // Explicitly set date range from January 2025 to March 2026 only
            var startMonth = new DateTime(2025, 1, 1);
            var endMonthExclusive = new DateTime(2026, 4, 1); // April 1, 2026 (exclusive) = up to March 31, 2026
            
            // Validate we don't exceed March 2026
            var maxAllowedDate = new DateTime(2026, 3, 31);

            // Get Finance Entries - strictly filtered by date range
            var financeQuery = _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived && f.ExpenseDate >= startMonth && f.ExpenseDate <= maxAllowedDate)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                financeQuery = financeQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var financeEntries = await financeQuery.ToListAsync();

            // Get Fuel Entries - strictly filtered by date range
            var fuelQuery = _context.FuelEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived && f.DateTime >= startMonth && f.DateTime <= maxAllowedDate)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                fuelQuery = fuelQuery.Where(f => f.BranchId == null || f.BranchId == branchId.Value);
            }

            var fuelEntries = await fuelQuery.ToListAsync();

            // Get Maintenance Entries - strictly filtered by date range
            var maintenanceQuery = _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => m.IsArchived != true && m.Date.HasValue && m.Date >= startMonth && m.Date <= maxAllowedDate)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                maintenanceQuery = maintenanceQuery.Where(m => m.BranchId == null || m.BranchId == branchId.Value);
            }

            var maintenanceEntries = await maintenanceQuery.ToListAsync();

            var data = new List<MonthlyExpenseBreakdownData>();
            // Generate data for all months from January 2025 to March 2026
            var currentMonth = startMonth;
            while (currentMonth < endMonthExclusive)
            {
                var monthStart = currentMonth;
                var monthEnd = currentMonth.AddMonths(1);

                // Calculate fuel costs for this month
                var fuelCost = fuelEntries
                    .Where(f => f.DateTime >= monthStart && f.DateTime < monthEnd)
                    .Sum(f => f.TotalCost);

                // Calculate maintenance costs for this month
                var maintenanceCost = maintenanceEntries
                    .Where(m => m.Date >= monthStart && m.Date < monthEnd)
                    .Sum(m => m.Cost ?? 0);

                // Calculate other finance costs for this month
                var otherCost = financeEntries
                    .Where(f => f.ExpenseDate >= monthStart && f.ExpenseDate < monthEnd)
                    .Sum(f => f.Amount);

                var totalCost = fuelCost + maintenanceCost + otherCost;

                data.Add(new MonthlyExpenseBreakdownData
                {
                    Month = currentMonth.ToString("MMM yyyy"),
                    FuelCost = (double)fuelCost,
                    MaintenanceCost = (double)maintenanceCost,
                    OtherCost = (double)otherCost,
                    TotalCost = (double)totalCost
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            return data;
        }

        private async Task<List<MonthlyBudgetData>> GetMonthlyBudgetsAsync(bool isSuperAdmin, int? branchId)
        {
            // Explicitly set date range from January 2025 to March 2026 only
            var startMonth = new DateTime(2025, 1, 1);
            var endMonthExclusive = new DateTime(2026, 4, 1); // April 1, 2026 (exclusive) = up to March 31, 2026
            
            // Validate we don't exceed March 2026
            var maxAllowedDate = new DateTime(2026, 3, 31);
            var data = new List<MonthlyBudgetData>();

            var budgetQuery = _context.BudgetEntries
                .AsNoTracking()
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                budgetQuery = budgetQuery.Where(b => b.BranchId == null || b.BranchId == branchId.Value);
            }

            var budgets = await budgetQuery.ToListAsync();

            var budgetLookup = budgets
                .Where(b => DateTime.TryParseExact(b.Month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
                .ToLookup(
                    b => b.Month,
                    b => b.BudgetAmount,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(), // Sum all budgets for the same month
                    StringComparer.OrdinalIgnoreCase);

            // Generate data for all months from January 2025 to March 2026
            var currentMonth = startMonth;
            while (currentMonth < endMonthExclusive)
            {
                var key = currentMonth.ToString("yyyy-MM");
                budgetLookup.TryGetValue(key, out var budgetAmount);

                data.Add(new MonthlyBudgetData
                {
                    Month = currentMonth.ToString("MMM yyyy"),
                    BudgetAmount = (double)budgetAmount
                });

                currentMonth = currentMonth.AddMonths(1);
            }

            return data;
        }

        private async Task<(bool IsSuperAdmin, int? BranchId)> GetBranchContextAsync()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            return (isSuperAdmin, user?.BranchId);
        }
    }

    public class ReportsViewModel
    {
        public List<string> ReportTypes { get; set; } = new List<string>();
        public string SelectedReportType { get; set; } = string.Empty;
        public string SelectedDateRange { get; set; } = string.Empty;
        public decimal TotalExpenses { get; set; } = 0;
        public decimal TotalFuelCost { get; set; } = 0;
        public decimal TotalMaintenanceCost { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;
        public List<ReportRecord> ReportRecords { get; set; } = new List<ReportRecord>();
        public List<MonthlyExpenseBreakdownData> MonthlyExpenseBreakdown { get; set; } = new List<MonthlyExpenseBreakdownData>();
        public decimal MonthlyBudget { get; set; } = 0;
        public List<MonthlyBudgetData> MonthlyBudgets { get; set; } = new List<MonthlyBudgetData>();
    }

    public class MonthlyBudgetData
    {
        public string Month { get; set; } = string.Empty;
        public double BudgetAmount { get; set; }
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
