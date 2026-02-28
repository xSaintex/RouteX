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

            var reportTypes = new List<string> { "Expenses", "Fuel", "Maintenance", "Vehicle" };
            var summary = await GetSummaryAsync();
            var monthlyBreakdown = await GetMonthlyExpenseBreakdownAsync();
            var monthlyBudgets = await GetMonthlyBudgetsAsync();
            var reportRecords = await GetReportRecordsAsync("Expenses", DateTime.Today.AddDays(-30), DateTime.Today);

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

            var (rangeStart, rangeEnd) = ResolveDateRange(dateRange, startDate, endDate);
            var records = await GetReportRecordsAsync(reportType, rangeStart, rangeEnd);

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

        private async Task<(decimal TotalExpenses, decimal TotalFuelCost, decimal TotalMaintenanceCost, int TotalTrips)> GetSummaryAsync()
        {
            var financeEntries = await _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived)
                .ToListAsync();

            var last24Start = DateTime.Today.AddMonths(-24);
            var totalExpenses = financeEntries
                .Where(f => f.ExpenseDate >= last24Start)
                .Sum(f => f.Amount);

            var totalFuel = await _context.FuelEntries
                .AsNoTracking()
                .SumAsync(f => f.TotalCost);

            var totalMaintenance = await _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => (m.IsArchived == null || m.IsArchived == false) && m.Status == (int)MaintenanceStatus.Completed)
                .SumAsync(m => m.Cost ?? 0);

            var totalTrips = await _context.Vehicles.AsNoTracking().CountAsync();

            return (totalExpenses, totalFuel, totalMaintenance, totalTrips);
        }

        private async Task<List<ReportRecord>> GetReportRecordsAsync(string reportType, DateTime startDate, DateTime endDate)
        {
            var endExclusive = endDate.AddDays(1);

            switch (reportType)
            {
                case "Fuel":
                    return await _context.FuelEntries
                        .AsNoTracking()
                        .Include(f => f.Vehicle)
                        .Where(f => !f.IsArchived && f.DateTime >= startDate && f.DateTime < endExclusive)
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
                    return await _context.MaintenanceEntries
                        .AsNoTracking()
                        .Include(m => m.Vehicle)
                        .Where(m => m.IsArchived != true && m.Date.HasValue && m.Date >= startDate && m.Date < endExclusive)
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
                    return await _context.Vehicles
                        .AsNoTracking()
                        .Where(v => !v.IsArchived)
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
                    return await _context.FinanceEntries
                        .AsNoTracking()
                        .Include(f => f.Vehicle)
                        .Where(f => !f.IsArchived && f.ExpenseDate >= startDate && f.ExpenseDate < endExclusive)
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

        private async Task<List<MonthlyExpenseBreakdownData>> GetMonthlyExpenseBreakdownAsync()
        {
            var startMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
            var endMonthExclusive = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);

            var financeEntries = await _context.FinanceEntries
                .AsNoTracking()
                .Where(f => !f.IsArchived && f.ExpenseDate >= startMonth && f.ExpenseDate < endMonthExclusive)
                .ToListAsync();

            var grouped = financeEntries
                .GroupBy(f => new { f.ExpenseDate.Year, f.ExpenseDate.Month })
                .ToDictionary(g => new DateTime(g.Key.Year, g.Key.Month, 1), g => g.ToList());

            var data = new List<MonthlyExpenseBreakdownData>();
            for (int i = 0; i < 6; i++)
            {
                var month = startMonth.AddMonths(i);
                grouped.TryGetValue(month, out var entries);
                entries ??= new List<FinanceEntry>();

                var fuel = entries
                    .Where(e => string.Equals(e.ExpenseType, "Fuel", StringComparison.OrdinalIgnoreCase))
                    .Sum(e => e.Amount);
                var maintenance = entries
                    .Where(e => string.Equals(e.ExpenseType, "Maintenance", StringComparison.OrdinalIgnoreCase))
                    .Sum(e => e.Amount);
                var other = entries
                    .Where(e => !string.Equals(e.ExpenseType, "Fuel", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(e.ExpenseType, "Maintenance", StringComparison.OrdinalIgnoreCase))
                    .Sum(e => e.Amount);

                data.Add(new MonthlyExpenseBreakdownData
                {
                    Month = month.ToString("MMM yyyy"),
                    FuelCost = (double)fuel,
                    MaintenanceCost = (double)maintenance,
                    OtherCost = (double)other,
                    TotalCost = (double)(fuel + maintenance + other)
                });
            }

            return data;
        }

        private async Task<List<MonthlyBudgetData>> GetMonthlyBudgetsAsync()
        {
            var startMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(-5);
            var data = new List<MonthlyBudgetData>();

            var budgets = await _context.BudgetEntries
                .AsNoTracking()
                .Where(b => b.IsActive)
                .ToListAsync();

            var budgetLookup = budgets
                .Where(b => DateTime.TryParseExact(b.Month + "-01", "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out _))
                .ToDictionary(
                    b => b.Month,
                    b => b.BudgetAmount,
                    StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < 6; i++)
            {
                var month = startMonth.AddMonths(i);
                var key = month.ToString("yyyy-MM");
                budgetLookup.TryGetValue(key, out var budgetAmount);

                data.Add(new MonthlyBudgetData
                {
                    Month = month.ToString("MMM yyyy"),
                    BudgetAmount = (double)budgetAmount
                });
            }

            return data;
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
