using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;

namespace RouteX.Controllers
{
    public class BranchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public BranchesController(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // GET: Branches
        public async Task<IActionResult> BranchesPage()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            ViewBag.UserRole = userRole;

            // Only SuperAdmin can view all branches
            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            var branches = await _context.Branches
                .AsNoTracking()
                .Where(b => !b.IsArchived)
                .OrderByDescending(b => b.BranchId)
                .ToListAsync();

            return View(branches);
        }

        // GET: Branches/ViewBranch/5
        public async Task<IActionResult> ViewBranch(int? id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            ViewBag.UserRole = userRole;

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches
                .Include(b => b.Users)
                .Include(b => b.Vehicles)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // GET: Branches/AddBranch
        public IActionResult AddBranch()
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            ViewBag.UserRole = userRole;

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: Branches/AddBranch
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBranch(Branch branch)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to perform this action.";
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                branch.CreatedAt = DateTime.UtcNow;
                branch.UpdatedAt = DateTime.UtcNow;
                branch.CreatedBy = userEmail;
                branch.UpdatedBy = userEmail;
                branch.IsArchived = false;

                _context.Add(branch);
                await _context.SaveChangesAsync();

                await _auditService.LogActionAsync(userEmail, $"Created branch: {branch.BranchName}");

                TempData["Success"] = "Branch added successfully!";
                return RedirectToAction(nameof(BranchesPage));
            }

            ViewBag.UserRole = userRole;
            return View(branch);
        }

        // GET: Branches/EditBranch/5
        public async Task<IActionResult> EditBranch(int? id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            ViewBag.UserRole = userRole;

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to access this page.";
                return RedirectToAction("Index", "Home");
            }

            if (id == null)
            {
                return NotFound();
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            return View(branch);
        }

        // POST: Branches/EditBranch/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBranch(int id, Branch branch)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Error"] = "You do not have permission to perform this action.";
                return RedirectToAction("Index", "Home");
            }

            if (id != branch.BranchId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingBranch = await _context.Branches.FindAsync(id);
                    if (existingBranch == null)
                    {
                        return NotFound();
                    }

                    existingBranch.BranchName = branch.BranchName;
                    existingBranch.Address = branch.Address;
                    existingBranch.City = branch.City;
                    existingBranch.Province = branch.Province;
                    existingBranch.PostalCode = branch.PostalCode;
                    existingBranch.PhoneNumber = branch.PhoneNumber;
                    existingBranch.Email = branch.Email;
                    existingBranch.Latitude = branch.Latitude;
                    existingBranch.Longitude = branch.Longitude;
                    existingBranch.CoverageRadiusKm = branch.CoverageRadiusKm;
                    existingBranch.Status = branch.Status;
                    existingBranch.ManagerName = branch.ManagerName;
                    existingBranch.OperatingHours = branch.OperatingHours;
                    existingBranch.ServiceAreas = branch.ServiceAreas;
                    existingBranch.UpdatedAt = DateTime.UtcNow;
                    existingBranch.UpdatedBy = userEmail;

                    await _context.SaveChangesAsync();

                                        await _auditService.LogActionAsync(userEmail, $"Updated branch: {branch.BranchName}");

                                        TempData["Success"] = "Branch updated successfully!";
                    return RedirectToAction(nameof(BranchesPage));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.UserRole = userRole;
            return View(branch);
        }

        // POST: Branches/ArchiveBranch/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveBranch(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "You do not have permission to perform this action." });
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return Json(new { success = false, message = "Branch not found." });
            }

            branch.IsArchived = true;
            branch.Status = BranchStatus.Archived;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();
            await _auditService.LogActionAsync(userEmail, $"Archived branch: {branch.BranchName}");

            return Json(new { success = true, message = "Branch archived successfully!" });
        }

        // POST: Branches/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";

            if (!userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "You do not have permission to perform this action." });
            }

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return Json(new { success = false, message = "Branch not found." });
            }

            branch.Status = branch.Status == BranchStatus.Active ? BranchStatus.Inactive : BranchStatus.Active;
            branch.UpdatedAt = DateTime.UtcNow;
            branch.UpdatedBy = userEmail;

            await _context.SaveChangesAsync();
            await _auditService.LogActionAsync(userEmail, $"Changed branch status: {branch.BranchName} to {branch.Status}");

            return Json(new { success = true, message = $"Branch status changed to {branch.Status}!", status = branch.Status.ToString() });
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }
    }
}
