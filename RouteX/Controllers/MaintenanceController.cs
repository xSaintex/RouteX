using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;

namespace RouteX.Controllers
{
    [Authorize]
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ITextFormattingService _textFormattingService;

        public MaintenanceController(ApplicationDbContext context, IAuditService auditService, ITextFormattingService textFormattingService)
        {
            _context = context;
            _auditService = auditService;
            _textFormattingService = textFormattingService;
        }

        // GET: Maintenance/MaintenancePage
        public async Task<IActionResult> MaintenancePage(string? status)
        {
            ViewData["Title"] = "Maintenance Schedule";
            MaintenanceStatus? parsedStatus = null;

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<MaintenanceStatus>(status, true, out var statusValue))
            {
                parsedStatus = statusValue;
                ViewBag.InitialStatusFilter = statusValue.ToString();
            }

            // Get user's branch for filtering
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var userBranchId = user?.BranchId;
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            
            ViewBag.UserRole = userRole;

            try
            {
                var query = _context.MaintenanceEntries
                    .AsNoTracking()
                    .Include(m => m.Branch)
                    .Where(m => m.IsArchived != true);

                // Filter by branch if not SuperAdmin
                if (!isSuperAdmin && userBranchId.HasValue)
                {
                    query = query.Where(m => m.BranchId == userBranchId.Value);
                }

                var maintenanceEntries = await query
                    .OrderByDescending(m => m.Id)
                    .ToListAsync();

                if (parsedStatus.HasValue)
                {
                    var statusFilterValue = (int)parsedStatus.Value;
                    maintenanceEntries = maintenanceEntries
                        .Where(m => (m.Status ?? (int)MaintenanceStatus.Pending) == statusFilterValue)
                        .ToList();
                }

                return View(maintenanceEntries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing MaintenanceEntries: {ex.Message}");
                return View(new List<MaintenanceEntry>());
            }
        }

        // GET: Maintenance/AddMaintenance
        public async Task<IActionResult> AddMaintenance()
        {
            ViewData["Title"] = "Add Maintenance";

            // Get user's branch for filtering
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var userBranchId = user?.BranchId;
            var isSuperAdmin = HttpContext.Session.GetString("UserRole")?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ?? false;

            try
            {
                var vehicleQuery = _context.Vehicles
                    .AsNoTracking()
                    .Where(v => !v.IsArchived);

                // Filter vehicles by branch if not SuperAdmin
                if (!isSuperAdmin && userBranchId.HasValue)
                {
                    vehicleQuery = vehicleQuery.Where(v => v.BranchId == userBranchId.Value);
                }

                var activeVehicles = await vehicleQuery
                    .OrderBy(v => v.PlateNumber)
                    .ToListAsync();

                ViewBag.Vehicles = activeVehicles;
                ViewBag.UserBranchId = userBranchId;
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing Vehicles for AddMaintenance: {ex.Message}");
                ViewBag.Vehicles = new List<Vehicle>();
                return View();
            }
        }

        // POST: Maintenance/AddMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaintenance(MaintenanceEntry maintenance)
        {
            // Validate NextServiceDue - cannot be past date or current date
            if (maintenance.NextServiceDue.HasValue)
            {
                var today = DateTime.Today;
                var nextServiceDate = maintenance.NextServiceDue.Value.Date;
                
                if (nextServiceDate <= today)
                {
                    ModelState.AddModelError("NextServiceDue", "Next service due date must be a future date (after today).");
                }
            }

            // Apply auto-capitalization to text fields
            if (!string.IsNullOrWhiteSpace(maintenance.ServiceType))
                maintenance.ServiceType = _textFormattingService.CapitalizeEachWord(maintenance.ServiceType);
            
            if (!string.IsNullOrWhiteSpace(maintenance.TechnicianName))
                maintenance.TechnicianName = _textFormattingService.FormatName(maintenance.TechnicianName);
            
            if (!string.IsNullOrWhiteSpace(maintenance.Description))
                maintenance.Description = _textFormattingService.CapitalizeFirstLetter(maintenance.Description);

            if (ModelState.IsValid)
            {
                try
                {
                    // Auto-assign branch from user
                    var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
                    var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                    var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
                    if (user?.BranchId.HasValue == true)
                    {
                        maintenance.BranchId = user.BranchId;
                    }

                    // Set vehicle details if VehicleId is provided
                    if (maintenance.VehicleId.HasValue)
                    {
                        var vehicles = ViewBag.Vehicles as List<RouteX.Models.Vehicle> ?? new List<RouteX.Models.Vehicle>();
                        var vehicle = vehicles.FirstOrDefault(v => v.Id == maintenance.VehicleId.Value);
                        if (vehicle != null)
                        {
                            if (!isSuperAdmin && user?.BranchId != vehicle.BranchId)
                            {
                                return Forbid();
                            }

                            maintenance.UnitModel = vehicle.UnitModel;
                            maintenance.PlateNumber = vehicle.PlateNumber;
                            maintenance.BranchId = vehicle.BranchId;
                        }
                        else
                        {
                            // If vehicle not found in ViewBag, fetch from database
                            var dbVehicle = await _context.Vehicles.FindAsync(maintenance.VehicleId.Value);
                            if (dbVehicle != null)
                            {
                                if (!isSuperAdmin && user?.BranchId != dbVehicle.BranchId)
                                {
                                    return Forbid();
                                }

                                maintenance.UnitModel = dbVehicle.UnitModel;
                                maintenance.PlateNumber = dbVehicle.PlateNumber;
                                maintenance.BranchId = dbVehicle.BranchId;
                            }
                        }
                    }
                    else
                    {
                        // If no VehicleId provided, ensure PlateNumber is not null
                        if (string.IsNullOrEmpty(maintenance.PlateNumber))
                        {
                            ModelState.AddModelError("VehicleId", "Please select a vehicle or provide a plate number.");
                        }
                    }

                    // Set Date property for backward compatibility
                    maintenance.Date = maintenance.ServiceDate;
                    maintenance.IsArchived = false;

                    // Final validation - ensure PlateNumber is not null
                    if (string.IsNullOrEmpty(maintenance.PlateNumber))
                    {
                        ModelState.AddModelError("VehicleId", "Plate number is required. Please select a valid vehicle.");
                        var vehicleOptions = await _context.Vehicles
                            .AsNoTracking()
                            .Where(v => !v.IsArchived)
                            .OrderBy(v => v.PlateNumber)
                            .ToListAsync();
                        
                        ViewBag.Vehicles = vehicleOptions;
                        return View(maintenance);
                    }

                    // Use raw SQL to avoid OUTPUT clause issues with triggers
                    var sql = @"INSERT INTO MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, IsArchived, MaintenanceId, UnitModel, Date, BranchId)
                              VALUES (@VehicleId, @PlateNumber, @ServiceType, @ServiceDate, @Cost, @TechnicianName, @Status, @OdometerAtService, @NextServiceDue, @Description, @IsArchived, @MaintenanceId, @UnitModel, @Date, @BranchId);
                              SELECT CAST(SCOPE_IDENTITY() as int);";
                    
                    var parameters = new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@VehicleId", maintenance.VehicleId ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@PlateNumber", maintenance.PlateNumber ?? string.Empty),
                        new Microsoft.Data.SqlClient.SqlParameter("@ServiceType", maintenance.ServiceType ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@ServiceDate", maintenance.ServiceDate == default ? (object)DBNull.Value : maintenance.ServiceDate),
                        new Microsoft.Data.SqlClient.SqlParameter("@Cost", maintenance.Cost ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@TechnicianName", maintenance.TechnicianName ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Status", maintenance.Status ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@OdometerAtService", maintenance.OdometerAtService ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@NextServiceDue", maintenance.NextServiceDue ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Description", maintenance.Description ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@IsArchived", maintenance.IsArchived),
                        new Microsoft.Data.SqlClient.SqlParameter("@MaintenanceId", maintenance.MaintenanceId ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@UnitModel", maintenance.UnitModel ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Date", maintenance.ServiceDate == default ? (object)DBNull.Value : maintenance.ServiceDate),
                        new Microsoft.Data.SqlClient.SqlParameter("@BranchId", maintenance.BranchId ?? (object)DBNull.Value)
                    };
                    
                    var id = await _context.Database.ExecuteSqlRawAsync(sql, parameters);
                    maintenance.Id = id;

                    if (maintenance.VehicleId.HasValue)
                    {
                        await UpdateVehicleStatusBasedOnAllMaintenances(maintenance.VehicleId.Value);
                    }

                    var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
                    await _auditService.LogActionAsync(actingUser, $"Create:Maintenance:{maintenance.Id}");

                    TempData["Success"] = "Maintenance schedule added successfully!";
                    TempData["RecentMaintenanceId"] = maintenance.Id;
                    return RedirectToAction(nameof(MaintenancePage));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error adding maintenance: {ex.Message}");
                    TempData["Error"] = $"Error adding maintenance: {ex.Message}";
                }
            }

            var activeVehicles = await _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .OrderBy(v => v.PlateNumber)
                .ToListAsync();
            
            ViewBag.Vehicles = activeVehicles;
            return View(maintenance);
        }

        // GET: Maintenance/EditMaintenance/5
        public async Task<IActionResult> EditMaintenance(int id)
        {
            ViewData["Title"] = "Edit Maintenance";

            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            
            ViewBag.UserRole = userRole;
            
            try
            {
                var maintenance = await _context.MaintenanceEntries
                    .AsNoTracking()
                    .Include(m => m.Vehicle)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (maintenance == null)
                {
                    TempData["Error"] = "Maintenance schedule not found.";
                    return RedirectToAction(nameof(MaintenancePage));
                }

                if (!isSuperAdmin && user?.BranchId != maintenance.BranchId)
                {
                    return Forbid();
                }

                var activeVehicles = await _context.Vehicles
                    .AsNoTracking()
                    .Where(v => !v.IsArchived)
                    .Where(v => isSuperAdmin || v.BranchId == user!.BranchId)
                    .OrderBy(v => v.PlateNumber)
                    .ToListAsync();
                
                ViewBag.Vehicles = activeVehicles;
                return View(maintenance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error accessing MaintenanceEntries for EditMaintenance: {ex.Message}");
                TempData["Error"] = "Error accessing maintenance data. The maintenance table may not be available.";
                return RedirectToAction(nameof(MaintenancePage));
            }
        }

        // POST: Maintenance/EditMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMaintenance(int id, MaintenanceEntry maintenance)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            
            ViewBag.UserRole = userRole;

            // Validate NextServiceDue - cannot be past date or current date
            if (maintenance.NextServiceDue.HasValue)
            {
                var today = DateTime.Today;
                var nextServiceDate = maintenance.NextServiceDue.Value.Date;
                
                if (nextServiceDate <= today)
                {
                    ModelState.AddModelError("NextServiceDue", "Next service due date must be a future date (after today).");
                }
            }

            // Apply auto-capitalization to text fields
            if (!string.IsNullOrWhiteSpace(maintenance.ServiceType))
                maintenance.ServiceType = _textFormattingService.CapitalizeEachWord(maintenance.ServiceType);
            
            if (!string.IsNullOrWhiteSpace(maintenance.TechnicianName))
                maintenance.TechnicianName = _textFormattingService.FormatName(maintenance.TechnicianName);
            
            if (!string.IsNullOrWhiteSpace(maintenance.Description))
                maintenance.Description = _textFormattingService.CapitalizeFirstLetter(maintenance.Description);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                Console.WriteLine("Edit Maintenance ModelState errors: " + string.Join(", ", errors));
                TempData["Error"] = "Please fix the validation errors: " + string.Join(", ", errors);
            }

            if (id != maintenance.Id)
            {
                TempData["Error"] = "Maintenance ID mismatch.";
                return RedirectToAction(nameof(MaintenancePage));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.MaintenanceEntries
                        .AsNoTracking()
                        .FirstOrDefaultAsync(m => m.Id == id);
                    if (existing == null)
                    {
                        TempData["Error"] = "Maintenance schedule not found.";
                        return RedirectToAction(nameof(MaintenancePage));
                    }

                    if (!isSuperAdmin && user?.BranchId != existing.BranchId)
                    {
                        return Forbid();
                    }

                    maintenance.BranchId = existing.BranchId;

                    var vehicle = maintenance.VehicleId.HasValue
                        ? await _context.Vehicles.FindAsync(maintenance.VehicleId.Value)
                        : null;
                    if (vehicle != null)
                    {
                        if (!isSuperAdmin && user?.BranchId != vehicle.BranchId)
                        {
                            return Forbid();
                        }

                        maintenance.UnitModel = vehicle.UnitModel;
                        maintenance.PlateNumber = vehicle.PlateNumber;
                    }
                    else if (maintenance.VehicleId.HasValue)
                    {
                        // VehicleId provided but vehicle not found
                        ModelState.AddModelError("VehicleId", "Selected vehicle not found.");
                        var vehicleList = await _context.Vehicles
                            .AsNoTracking()
                            .Where(v => !v.IsArchived)
                            .Where(v => isSuperAdmin || v.BranchId == user!.BranchId)
                            .OrderBy(v => v.PlateNumber)
                            .ToListAsync();
                        
                        ViewBag.Vehicles = vehicleList;
                        return View(maintenance);
                    }
                    else
                    {
                        // No VehicleId provided, ensure PlateNumber is not null
                        if (string.IsNullOrEmpty(maintenance.PlateNumber))
                        {
                            ModelState.AddModelError("VehicleId", "Please select a vehicle or provide a plate number.");
                            var vehicleList = await _context.Vehicles
                                .AsNoTracking()
                                .Where(v => !v.IsArchived)
                                .Where(v => isSuperAdmin || v.BranchId == user!.BranchId)
                                .OrderBy(v => v.PlateNumber)
                                .ToListAsync();
                            
                            ViewBag.Vehicles = vehicleList;
                            return View(maintenance);
                        }
                    }

                    maintenance.MaintenanceId = existing.MaintenanceId;
                    maintenance.IsArchived = existing.IsArchived;

                    var sql = @"UPDATE MaintenanceEntries
                              SET VehicleId = @VehicleId,
                                  PlateNumber = @PlateNumber,
                                  ServiceType = @ServiceType,
                                  ServiceDate = @ServiceDate,
                                  Cost = @Cost,
                                  TechnicianName = @TechnicianName,
                                  Status = @Status,
                                  OdometerAtService = @OdometerAtService,
                                  NextServiceDue = @NextServiceDue,
                                  Description = @Description,
                                  MaintenanceId = @MaintenanceId,
                                  UnitModel = @UnitModel,
                                   BranchId = @BranchId,
                                  Date = @Date,
                                  ModifiedDate = GETDATE()
                              WHERE Id = @Id";

                    var parameters = new[]
                    {
                        new Microsoft.Data.SqlClient.SqlParameter("@VehicleId", maintenance.VehicleId ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@PlateNumber", maintenance.PlateNumber ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@ServiceType", maintenance.ServiceType ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@ServiceDate", maintenance.ServiceDate == default ? (object)DBNull.Value : maintenance.ServiceDate),
                        new Microsoft.Data.SqlClient.SqlParameter("@Cost", maintenance.Cost ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@TechnicianName", maintenance.TechnicianName ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Status", maintenance.Status ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@OdometerAtService", maintenance.OdometerAtService ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@NextServiceDue", maintenance.NextServiceDue ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Description", maintenance.Description ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@MaintenanceId", maintenance.MaintenanceId ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@UnitModel", maintenance.UnitModel ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@BranchId", maintenance.BranchId ?? (object)DBNull.Value),
                        new Microsoft.Data.SqlClient.SqlParameter("@Date", maintenance.ServiceDate == default ? (object)DBNull.Value : maintenance.ServiceDate),
                        new Microsoft.Data.SqlClient.SqlParameter("@Id", maintenance.Id)
                    };

                    await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                    // Check if maintenance status was updated to Completed
                    if (maintenance.Status == (int)MaintenanceStatus.Completed)
                    {
                        await CreateFinanceEntryFromMaintenance(maintenance);
                    }

                    if (maintenance.VehicleId.HasValue)
                    {
                        await UpdateVehicleStatusBasedOnAllMaintenances(maintenance.VehicleId.Value);
                    }

                    var actingUser = userEmail;
                    await _auditService.LogActionAsync(actingUser, $"Update:Maintenance:{maintenance.Id}");

                    TempData["Success"] = "Maintenance schedule updated successfully!";
                    TempData["RecentMaintenanceId"] = maintenance.Id;
                    TempData["RecentMaintenanceAction"] = "Edited";
                    return RedirectToAction(nameof(MaintenancePage));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating maintenance: {ex.Message}");
                }
            }

            var activeVehicles = await _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived)
                .Where(v => isSuperAdmin || v.BranchId == user!.BranchId)
                .OrderBy(v => v.PlateNumber)
                .ToListAsync();
            
            ViewBag.Vehicles = activeVehicles;
            return View(maintenance);
        }

        // GET: Maintenance/ViewMaintenance/5
        public async Task<IActionResult> ViewMaintenance(int id)
        {
            ViewData["Title"] = "View Maintenance";

            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);
            
            var maintenance = await _context.MaintenanceEntries
                .AsNoTracking()
                .Include(m => m.Vehicle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenance == null)
            {
                TempData["Error"] = "Maintenance schedule not found.";
                return RedirectToAction(nameof(MaintenancePage));
            }

            if (!isSuperAdmin && user?.BranchId != maintenance.BranchId)
            {
                return Forbid();
            }

            return View(maintenance);
        }

        private async Task UpdateVehicleStatusBasedOnAllMaintenances(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null) return;

            // Get all non-archived maintenance entries for this vehicle
            var allMaintenances = await _context.MaintenanceEntries
                .AsNoTracking()
                .Where(m => m.VehicleId == vehicleId && m.IsArchived != true)
                .ToListAsync();

            if (allMaintenances.Any())
            {
                // Check if ALL maintenance entries are completed
                var allCompleted = allMaintenances.All(m => m.Status == (int)MaintenanceStatus.Completed);
                vehicle.Status = allCompleted ? VehicleStatus.Active : VehicleStatus.Maintenance;
            }
            else
            {
                // No maintenance entries, set to Active
                vehicle.Status = VehicleStatus.Active;
            }

            await _context.SaveChangesAsync();
        }

        private async Task CreateFinanceEntryFromMaintenance(MaintenanceEntry maintenance)
        {
            try
            {
                var financeEntry = new FinanceEntry
                {
                    VehicleId = maintenance.VehicleId ?? 0,
                    ExpenseType = "Maintenance",
                    Amount = maintenance.Cost ?? 0,
                    ExpenseDate = maintenance.ServiceDate,
                    Description = !string.IsNullOrEmpty(maintenance.Description) ? maintenance.Description : null,
                    ReferenceId = maintenance.Id,
                    BranchId = maintenance.BranchId,
                    IsArchived = false
                };

                _context.FinanceEntries.Add(financeEntry);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error but don't fail the maintenance entry creation
                Console.WriteLine($"Error creating finance entry from maintenance: {ex.Message}");
            }
        }

        // POST: Maintenance/ArchiveMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveMaintenance(int id)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? string.Empty;
            var userRole = HttpContext.Session.GetString("UserRole") ?? string.Empty;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userEmail);
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

            var maintenance = await _context.MaintenanceEntries.FindAsync(id);
            if (maintenance == null)
            {
                return Json(new { success = false, message = "Maintenance schedule not found." });
            }

            if (!isSuperAdmin && user?.BranchId != maintenance.BranchId)
            {
                return Json(new { success = false, message = "You do not have access to this branch data." });
            }

            try
            {
                var sql = "UPDATE MaintenanceEntries SET IsArchived = 1 WHERE Id = @Id";
                var parameters = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@Id", maintenance.Id)
                };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                var financeSql = "UPDATE FinanceEntries SET IsArchived = 1 WHERE ReferenceId = @Id AND (ExpenseType = 'Maintenance' OR ExpenseType = 'MAINTENANCE')";
                await _context.Database.ExecuteSqlRawAsync(financeSql, parameters);

                // If this maintenance entry was for a vehicle, check if we need to update the vehicle status
                if (maintenance.VehicleId.HasValue)
                {
                    await UpdateVehicleStatusBasedOnAllMaintenances(maintenance.VehicleId.Value);
                }

                var archivedBy = userEmail;
                await _auditService.LogActionAsync(archivedBy, $"Archive:Maintenance:{maintenance.Id}:Status:{maintenance.Status ?? 0}");

                return Json(new { success = true, message = "Maintenance schedule archived successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error archiving maintenance: {ex.Message}" });
            }
        }
    }
}
