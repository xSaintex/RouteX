using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using RouteX.ViewModels;

namespace RouteX.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public BudgetController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(BudgetPage));
        }

        [HttpGet]
        public async Task<IActionResult> BudgetPage()
        {
            ViewData["Title"] = "Budget";

            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            var branchId = user?.BranchId;
            
            ViewBag.UserRole = userRole;

            var query = _context.BudgetEntries
                .AsNoTracking()
                .Include(b => b.Branch)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (!isSuperAdmin && branchId.HasValue)
            {
                query = query.Where(b => b.BranchId == branchId.Value);
            }

            var entries = await query.OrderByDescending(b => b.Month).ToListAsync();

            var viewModel = new BudgetPageViewModel
            {
                Entries = entries
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BudgetPage(BudgetPageViewModel model)
        {
            ViewData["Title"] = "Budget";

            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            var resolvedBranchId = user?.BranchId ?? await _context.Branches.AsNoTracking().Where(b => !b.IsArchived).Select(b => (int?)b.BranchId).FirstOrDefaultAsync();

            if (!resolvedBranchId.HasValue)
            {
                ModelState.AddModelError(string.Empty, "No active branch available for budget assignment.");
            }

            if (model.MonthValue == null || model.Year == null || model.Amount == null)
            {
                ModelState.AddModelError(string.Empty, "Month, year, and budget amount are required.");
            }

            var createdBy = userEmail;
            var monthValue = Math.Clamp(model.MonthValue ?? 1, 1, 12);
            var yearValue = model.Year ?? DateTime.Today.Year;
            var monthLabel = $"{yearValue:D4}-{monthValue:D2}";

            var duplicateExists = await _context.BudgetEntries
                .AsNoTracking()
                .AnyAsync(b => b.Month == monthLabel && b.IsActive && b.BranchId == resolvedBranchId);
            if (duplicateExists)
            {
                ModelState.AddModelError(string.Empty, "Budget for the selected month already exists.");
            }

            if (!ModelState.IsValid)
            {
                model.Entries = await _context.BudgetEntries
                    .AsNoTracking()
                    .Where(b => b.IsActive)
                    .Where(b => isSuperAdmin || b.BranchId == resolvedBranchId)
                    .OrderByDescending(b => b.Month)
                    .ToListAsync();

                return View(model);
            }

            var entry = new BudgetEntry
            {
                Month = monthLabel,
                BudgetAmount = model.Amount ?? 0m,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                BranchId = resolvedBranchId
            };

            _context.BudgetEntries.Add(entry);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Budget saved successfully.";
            return RedirectToAction(nameof(BudgetPage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateBudget(int id, int monthValue, int year, decimal amount)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

            var entry = await _context.BudgetEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            if (!isSuperAdmin && user?.BranchId != entry.BranchId)
            {
                return Forbid();
            }

            monthValue = Math.Clamp(monthValue, 1, 12);
            var monthLabel = $"{year:D4}-{monthValue:D2}";

            var duplicateExists = await _context.BudgetEntries
                .AsNoTracking()
                .AnyAsync(b => b.Id != id && b.Month == monthLabel && b.IsActive && b.BranchId == entry.BranchId);
            if (duplicateExists)
            {
                TempData["Error"] = "Budget for the selected month already exists.";
                return RedirectToAction(nameof(BudgetPage));
            }

            entry.Month = monthLabel;
            entry.BudgetAmount = amount;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Budget updated successfully.";
            return RedirectToAction(nameof(BudgetPage));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveBudget(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

            var entry = await _context.BudgetEntries.FindAsync(id);
            if (entry == null)
            {
                return NotFound();
            }

            if (!isSuperAdmin && user?.BranchId != entry.BranchId)
            {
                return Forbid();
            }

            entry.IsActive = false;
            await _context.SaveChangesAsync();

            var archivedBy = userEmail;
            await _auditService.LogActionAsync(archivedBy, $"Archive:Budget:{entry.Id}:Month:{entry.Month}");

            TempData["Success"] = "Budget archived successfully.";
            return RedirectToAction(nameof(BudgetPage));
        }
    }
}
