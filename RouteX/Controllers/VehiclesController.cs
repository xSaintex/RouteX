using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Net.Http;
using RouteX.Data;
using RouteX.Models;
using RouteX.Services;
using RouteX.ViewModels;

namespace RouteX.Controllers
{
    public class VehiclesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IConfiguration _configuration;
        private readonly IRouteDistanceService _routeDistanceService;
        private readonly ILogger<VehiclesController> _logger;
        private readonly HttpClient _httpClient;

        public VehiclesController(ApplicationDbContext context, IAuditService auditService, IConfiguration configuration, IRouteDistanceService routeDistanceService, ILogger<VehiclesController> logger, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _auditService = auditService;
            _configuration = configuration;
            _routeDistanceService = routeDistanceService;
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IActionResult> VehiclePage(string? status)
        {
            ViewData["Title"] = "Vehicle Management";

            var userRole = HttpContext.Session.GetString("UserRole") ?? "";
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            ViewBag.UserRole = userRole;

            // Get user's branch for filtering
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            var userBranchId = user?.BranchId;
            var isSuperAdmin = userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase);

            // Base query - exclude archived and pending approval vehicles for main list
            var query = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived && !v.IsPendingApproval);

            // Filter by branch if not SuperAdmin
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                query = query.Where(v => v.BranchId == userBranchId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VehicleStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(v => v.Status == parsedStatus);
                ViewBag.InitialStatusFilter = parsedStatus.ToString();
            }

            var vehicles = await query
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            // For Admin/SuperAdmin/Administrator: Get pending approval vehicles
            if (userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) || 
                userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                var pendingQuery = _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.IsPendingApproval && !v.IsArchived);

                // Filter by branch if not SuperAdmin
                if (!isSuperAdmin && userBranchId.HasValue)
                {
                    pendingQuery = pendingQuery.Where(v => v.BranchId == userBranchId.Value);
                }

                var pendingVehicles = await pendingQuery
                    .OrderByDescending(v => v.CreatedDate)
                    .ToListAsync();
                ViewBag.PendingVehicles = pendingVehicles;
            }

            // For OperationsStaff: Check if any of their vehicles were recently approved
            // Also show their own pending vehicles
            if (userRole.Equals("OperationsStaff", StringComparison.OrdinalIgnoreCase))
            {
                var recentlyApprovedQuery = _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.AddedByUserEmail == userEmail && 
                               !v.IsPendingApproval && 
                               v.ApprovalDate != null &&
                               v.ApprovalDate > DateTime.Now.AddDays(-1));

                // Filter by branch
                if (userBranchId.HasValue)
                {
                    recentlyApprovedQuery = recentlyApprovedQuery.Where(v => v.BranchId == userBranchId.Value);
                }

                var recentlyApprovedVehicles = await recentlyApprovedQuery.ToListAsync();
                ViewBag.RecentlyApprovedVehicles = recentlyApprovedVehicles;

                // Show their own pending vehicles
                var myPendingQuery = _context.Vehicles
                    .AsNoTracking()
                    .Where(v => v.AddedByUserEmail == userEmail && v.IsPendingApproval && !v.IsArchived);

                // Filter by branch
                if (userBranchId.HasValue)
                {
                    myPendingQuery = myPendingQuery.Where(v => v.BranchId == userBranchId.Value);
                }

                var myPendingVehicles = await myPendingQuery
                    .OrderByDescending(v => v.CreatedDate)
                    .ToListAsync();
                ViewBag.MyPendingVehicles = myPendingVehicles;
            }

            // Get mileage filtered by branch
            var mileageQuery = _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.Status == RouteTripStatus.Completed);

            // Filter by branch if not SuperAdmin
            if (!isSuperAdmin && userBranchId.HasValue)
            {
                mileageQuery = mileageQuery.Where(r => r.BranchId == userBranchId.Value);
            }

            var mileageByVehicleId = await mileageQuery
                .GroupBy(r => r.VehicleId)
                .Select(group => new
                {
                    VehicleId = group.Key,
                    TotalKm = group.Sum(r => r.DistanceKm ?? 0m)
                })
                .ToDictionaryAsync(item => item.VehicleId, item => item.TotalKm);

            ViewBag.MileageByVehicleId = mileageByVehicleId;

            return View(vehicles);
        }

        // GET: Vehicles/AddVehicle
        public async Task<IActionResult> AddVehicle()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "";
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);

            // Get branches for dropdown (SuperAdmin only)
            var isSuperAdmin = HttpContext.Session.GetString("UserRole")?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ?? false;
            if (isSuperAdmin)
            {
                var branches = await _context.Branches
                    .Where(b => !b.IsArchived && b.Status == BranchStatus.Active)
                    .OrderBy(b => b.BranchName)
                    .ToListAsync();
                ViewBag.Branches = branches;
            }

            ViewBag.UserBranchId = user?.BranchId;

            return View();
        }

        // POST: Vehicles/AddVehicle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVehicle(Vehicle vehicle)
        {
            if (ModelState.IsValid)
            {
                // Check for duplicate plate number
                var existingVehicle = await _context.Vehicles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.PlateNumber.ToLower() == vehicle.PlateNumber.ToLower() && v.Status != VehicleStatus.Archived);

                if (existingVehicle != null)
                {
                    ModelState.AddModelError("PlateNumber", "A vehicle with this plate number already exists.");
                    TempData["Error"] = "A vehicle with this plate number already exists.";
                    return View(vehicle);
                }

                var userRole = HttpContext.Session.GetString("UserRole") ?? "";
                var userEmail = HttpContext.Session.GetString("UserEmail") ?? "System";
                var userId = HttpContext.Session.GetString("UserId") ?? "";

                // Auto-assign branch from user (if not SuperAdmin with manual selection)
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
                if (user?.BranchId.HasValue == true && vehicle.BranchId == null)
                {
                    vehicle.BranchId = user.BranchId;
                }

                // Set created date
                vehicle.CreatedDate = DateTime.Now;

                // If user is OperationsStaff, set pending approval
                if (userRole.Equals("OperationsStaff", StringComparison.OrdinalIgnoreCase))
                {
                    vehicle.IsPendingApproval = true;
                    vehicle.AddedByUserId = userId;
                    vehicle.AddedByUserEmail = userEmail;

                    _context.Vehicles.Add(vehicle);
                    await _context.SaveChangesAsync();
                    await _auditService.LogActionAsync(userEmail, $"Create:Vehicle:Pending:{vehicle.Id}");
                    TempData["Success"] = "Vehicle submitted for approval! An admin will review your request.";
                    return RedirectToAction(nameof(VehiclePage));
                }

                // Admin/SuperAdmin - add directly
                vehicle.IsPendingApproval = false;
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                await _auditService.LogActionAsync(userEmail, $"Create:Vehicle:{vehicle.Id}");
                TempData["Success"] = "Vehicle added successfully!";
                TempData["RecentVehicleId"] = vehicle.Id;
                TempData["RecentAction"] = "Added";
                return RedirectToAction(nameof(VehiclePage));
            }

            return View(vehicle);
        }

        // GET: Vehicles/EditVehicle/5
        public async Task<IActionResult> EditVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        // POST: Vehicles/EditVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVehicle(int id, Vehicle vehicle)
        {
            if (id != vehicle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingVehicle = await _context.Vehicles.FindAsync(id);
                if (existingVehicle == null)
                {
                    return NotFound();
                }

                existingVehicle.UnitModel = vehicle.UnitModel;
                existingVehicle.PlateNumber = vehicle.PlateNumber;
                existingVehicle.VehicleType = vehicle.VehicleType;
                existingVehicle.Status = vehicle.Status;

                await _context.SaveChangesAsync();
                var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
                await _auditService.LogActionAsync(actingUser, $"Update:Vehicle:{existingVehicle.Id}");
                TempData["Success"] = "Vehicle updated successfully!";
                TempData["RecentVehicleId"] = vehicle.Id; // Track recently edited vehicle
                TempData["RecentAction"] = "Edited"; // Track action type
                return RedirectToAction(nameof(VehiclePage));
            }

            return View(vehicle);
        }

        // POST: Vehicles/ApproveVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveVehicle(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";

            // Only Admin, SuperAdmin, and Administrator can approve
            if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && 
                !userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
                !userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "You don't have permission to approve vehicles." });
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return Json(new { success = false, message = "Vehicle not found." });
            }

            if (!vehicle.IsPendingApproval)
            {
                return Json(new { success = false, message = "This vehicle is not pending approval." });
            }

            try
            {
                var approverEmail = HttpContext.Session.GetString("UserEmail") ?? "System";
                var approverId = HttpContext.Session.GetString("UserId") ?? "";

                vehicle.IsPendingApproval = false;
                vehicle.ApprovedByUserId = approverId;
                vehicle.ApprovedByUserEmail = approverEmail;
                vehicle.ApprovalDate = DateTime.Now;

                await _context.SaveChangesAsync();
                await _auditService.LogActionAsync(approverEmail, $"Approve:Vehicle:{vehicle.Id}:AddedBy:{vehicle.AddedByUserEmail}");

                return Json(new { success = true, message = $"Vehicle {vehicle.PlateNumber} has been approved and added to the fleet." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error approving vehicle: {ex.Message}" });
            }
        }

        // POST: Vehicles/RejectVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectVehicle(int id)
        {
            var userRole = HttpContext.Session.GetString("UserRole") ?? "";

            // Only Admin, SuperAdmin, and Administrator can reject
            if (!userRole.Equals("Admin", StringComparison.OrdinalIgnoreCase) && 
                !userRole.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) &&
                !userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
            {
                return Json(new { success = false, message = "You don't have permission to reject vehicles." });
            }

            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return Json(new { success = false, message = "Vehicle not found." });
            }

            if (!vehicle.IsPendingApproval)
            {
                return Json(new { success = false, message = "This vehicle is not pending approval." });
            }

            try
            {
                var rejectorEmail = HttpContext.Session.GetString("UserEmail") ?? "System";

                // Remove the rejected vehicle
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                await _auditService.LogActionAsync(rejectorEmail, $"Reject:Vehicle:{id}:AddedBy:{vehicle.AddedByUserEmail}");

                return Json(new { success = true, message = $"Vehicle {vehicle.PlateNumber} has been rejected and removed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error rejecting vehicle: {ex.Message}" });
            }
        }

        // POST: Vehicles/ArchiveVehicle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveVehicle(int id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                return Json(new { success = false, message = "Vehicle not found." });
            }

            try
            {
                var sql = "UPDATE Vehicles SET IsArchived = 1 WHERE VehicleId = @VehicleId";
                var parameters = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@VehicleId", vehicle.Id)
                };

                await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                var archivedBy = HttpContext.Session.GetString("UserEmail") ?? "System";
                await _auditService.LogActionAsync(archivedBy, $"Archive:Vehicle:{vehicle.Id}:Status:{(int)vehicle.Status}");

                return Json(new { success = true, message = $"{vehicle.UnitModel} has been archived successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error archiving vehicle: {ex.Message}" });
            }
        }

        // GET: Vehicles/ViewVehicle/5
        public async Task<IActionResult> ViewVehicle(int id)
        {
            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        [HttpGet]
        public async Task<IActionResult> RoutePage(int id)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
            {
                return NotFound();
            }

            // Check and add missing archive columns if needed
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RouteTrips' AND COLUMN_NAME = 'IsArchived')
                      ALTER TABLE RouteTrips ADD IsArchived BIT NOT NULL DEFAULT 0");

                await _context.Database.ExecuteSqlRawAsync(
                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RouteTrips' AND COLUMN_NAME = 'ArchivedAt')
                      ALTER TABLE RouteTrips ADD ArchivedAt DATETIME2 NULL");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking/adding archive columns");
                // Don't fail the request, just log the error
            }

            var currentRoute = await _context.RouteTrips
                .Where(r => r.VehicleId == id && r.Status == RouteTripStatus.Saved)
                .FirstOrDefaultAsync();

            var tripHistory = await _context.RouteTrips
                .Where(r => r.VehicleId == id && r.Status == RouteTripStatus.Completed && !r.IsArchived)
                .OrderByDescending(r => r.CompletedAt)
                .ToListAsync();

            var viewModel = new RoutePageViewModel
            {
                Vehicle = vehicle,
                CurrentRoute = currentRoute,
                TripHistory = tripHistory
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTrip(int vehicleId, string startAddress, string endAddress)
        {
            if (string.IsNullOrWhiteSpace(startAddress) || string.IsNullOrWhiteSpace(endAddress))
            {
                TempData["Error"] = "Start and end destinations are required.";
                return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
            }

            // Check vehicle status
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return NotFound();
            }

            if (vehicle.Status != VehicleStatus.Active)
            {
                TempData["Error"] = $"Cannot start trip: Vehicle is currently {vehicle.Status}. Only active vehicles can start new trips.";
                return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
            }

            // Cancel any existing saved routes for this vehicle
            var existingRoutes = await _context.RouteTrips
                .Where(r => r.VehicleId == vehicleId && r.Status == RouteTripStatus.Saved)
                .ToListAsync();

            foreach (var route in existingRoutes)
            {
                route.Status = RouteTripStatus.Cancelled;
                route.CancelledAt = DateTime.UtcNow;
            }

            // Create new saved route (ongoing trip)
            var newRoute = new RouteTrip
            {
                VehicleId = vehicleId,
                StartAddress = startAddress.Trim(),
                EndAddress = endAddress.Trim(),
                Status = RouteTripStatus.Saved,
                CreatedAt = DateTime.UtcNow
            };

            _context.RouteTrips.Add(newRoute);
            await _context.SaveChangesAsync();

            // Add audit log
            await _auditService.LogActionAsync("Added a Route Trip", $"Started trip from {startAddress} to {endAddress} for vehicle {vehicleId}");

            TempData["Success"] = "Trip started successfully!";
            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTrip(int vehicleId, string startAddress, string endAddress, decimal? distanceKm)
        {
            if (string.IsNullOrWhiteSpace(startAddress) || string.IsNullOrWhiteSpace(endAddress))
            {
                TempData["Error"] = "Start and end destinations are required.";
                return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
            }

            var existingRoutes = await _context.RouteTrips
                .Where(r => r.VehicleId == vehicleId && r.Status == RouteTripStatus.Saved)
                .ToListAsync();

            foreach (var route in existingRoutes)
            {
                route.Status = RouteTripStatus.Cancelled;
                route.CancelledAt = DateTime.UtcNow;
            }

            // Use the mileage provided by the frontend (JavaScript OSRM calculation)
            // If no mileage provided, try to calculate it on server
            if (!distanceKm.HasValue)
            {
                _logger.LogInformation("No mileage provided by frontend, attempting server calculation");
                distanceKm = await CalculateDistanceFromServer(startAddress, endAddress);
            }
            else
            {
                _logger.LogInformation("Using frontend-provided mileage: {Distance} km", distanceKm);
            }

            var now = DateTime.UtcNow;

            var newRoute = new RouteTrip
            {
                VehicleId = vehicleId,
                StartAddress = startAddress.Trim(),
                EndAddress = endAddress.Trim(),
                Status = RouteTripStatus.Completed,
                CreatedAt = now,
                CompletedAt = now,
                DistanceKm = distanceKm
            };

            _context.RouteTrips.Add(newRoute);
            await _context.SaveChangesAsync();

            // Add audit log for completed trip
            await _auditService.LogActionAsync("Completed a Trip", $"Completed trip from {startAddress} to {endAddress} for vehicle {vehicleId} with distance {distanceKm:N2} km");

            TempData["Success"] = distanceKm.HasValue
                ? $"Trip completed. Distance: {distanceKm.Value:N2} km."
                : "Trip completed.";
            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTrip(int tripId, int vehicleId)
        {
            try
            {
                var trip = await _context.RouteTrips.FindAsync(tripId);
                if (trip == null)
                {
                    return Json(new { success = false, message = "Trip not found." });
                }

                if (trip.IsArchived)
                {
                    return Json(new { success = false, message = "Trip is already archived." });
                }

                trip.IsArchived = true;
                trip.ArchivedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                // Add audit log
                var archivedBy = HttpContext.Session.GetString("UserEmail") ?? "System";
                await _auditService.LogActionAsync(archivedBy, $"Archive:Trip:{trip.Id}");

                return Json(new { success = true, message = "Trip archived successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving trip {TripId}", tripId);
                return Json(new { success = false, message = "Error archiving trip." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CalculateDistance([FromBody] CalculateDistanceRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.StartAddress) || string.IsNullOrWhiteSpace(request.EndAddress))
                {
                    return Json(new { success = false, error = "Start and end addresses are required." });
                }

                var distanceKm = await _routeDistanceService.GetDistanceKmAsync(request.StartAddress, request.EndAddress, HttpContext.RequestAborted);
                
                if (distanceKm.HasValue)
                {
                    return Json(new { success = true, distanceKm = distanceKm.Value });
                }
                else
                {
                    return Json(new { success = false, error = "Unable to calculate distance. Please check addresses." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance for {Start} to {End}", request?.StartAddress, request?.EndAddress);
                return Json(new { success = false, error = "An error occurred while calculating distance." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRoute(int vehicleId, string startAddress, string endAddress)
        {
            if (string.IsNullOrWhiteSpace(startAddress) || string.IsNullOrWhiteSpace(endAddress))
            {
                TempData["Error"] = "Start and end destinations are required.";
                return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
            }

            var existingRoutes = await _context.RouteTrips
                .Where(r => r.VehicleId == vehicleId && r.Status == RouteTripStatus.Saved)
                .ToListAsync();

            foreach (var route in existingRoutes)
            {
                route.Status = RouteTripStatus.Cancelled;
                route.CancelledAt = DateTime.UtcNow;
            }

            var newRoute = new RouteTrip
            {
                VehicleId = vehicleId,
                StartAddress = startAddress.Trim(),
                EndAddress = endAddress.Trim(),
                Status = RouteTripStatus.Saved,
                CreatedAt = DateTime.UtcNow
            };

            _context.RouteTrips.Add(newRoute);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Route saved successfully.";
            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteRoute(int routeId, int vehicleId)
        {
            var route = await _context.RouteTrips.FindAsync(routeId);
            if (route == null)
            {
                return NotFound();
            }

            var distanceKm = await _routeDistanceService.GetDistanceKmAsync(route.StartAddress, route.EndAddress, HttpContext.RequestAborted);

            route.Status = RouteTripStatus.Completed;
            route.CompletedAt = DateTime.UtcNow;
            route.DistanceKm = distanceKm;
            await _context.SaveChangesAsync();

            TempData["Success"] = distanceKm.HasValue
                ? $"Route marked as completed. Distance: {distanceKm.Value:N2} km."
                : "Route marked as completed.";
            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRoute(int routeId, int vehicleId)
        {
            var route = await _context.RouteTrips.FindAsync(routeId);
            if (route == null)
            {
                return NotFound();
            }

            route.Status = RouteTripStatus.Cancelled;
            route.CancelledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Route cancelled.";
            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearTripHistory(int vehicleId)
        {
            try
            {
                var trips = await _context.RouteTrips
                    .Where(r => r.VehicleId == vehicleId)
                    .ToListAsync();

                _context.RouteTrips.RemoveRange(trips);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cleared {trips.Count} trip records.";
                _logger.LogInformation("Cleared {Count} trip records for vehicle {VehicleId}", trips.Count, vehicleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing trip history for vehicle {VehicleId}", vehicleId);
                TempData["Error"] = "Error clearing trip history.";
            }

            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }

        private async Task<decimal?> CalculateDistanceFromServer(string startAddress, string endAddress)
        {
            try
            {
                // Use OSRM API for distance calculation (same as JavaScript)
                var startCoordinates = await GetCoordinatesFromOSRM(startAddress);
                var endCoordinates = await GetCoordinatesFromOSRM(endAddress);

                if (startCoordinates != null && endCoordinates != null)
                {
                    // Use the exact same OSRM URL format as JavaScript
                    var osrmUrl = $"https://router.project-osrm.org/route/v1/driving/{startCoordinates.Item2},{startCoordinates.Item1};{endCoordinates.Item2},{endCoordinates.Item1}?overview=full&geometries=geojson";
                    
                    var response = await _httpClient.GetAsync(osrmUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var osrmData = JsonSerializer.Deserialize<OSRMResponse>(jsonContent, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        var distanceMeters = osrmData?.Routes?.FirstOrDefault()?.Distance;
                        if (distanceMeters.HasValue)
                        {
                            return (decimal)distanceMeters.Value / 1000m;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance using OSRM for {Start} to {End}", startAddress, endAddress);
            }

            // Fallback: Use TomTom if OSRM fails
            try
            {
                return await _routeDistanceService.GetDistanceKmAsync(startAddress, endAddress, HttpContext.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance using TomTom for {Start} to {End}", startAddress, endAddress);
            }

            // Final fallback: Estimate based on cities
            return EstimateDistanceFromAddresses(startAddress, endAddress);
        }

        private async Task<Tuple<double?, double?>?> GetCoordinatesFromOSRM(string address)
        {
            try
            {
                var geocodeUrl = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(address)}&limit=1";
                _logger.LogInformation("Geocoding address: {Address}, URL: {Url}", address, geocodeUrl);
                
                var response = await _httpClient.GetAsync(geocodeUrl);
                _logger.LogInformation("Geocode response status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Geocode response: {Response}", jsonContent);
                    
                    var geocodeData = JsonSerializer.Deserialize<List<NominatimResponse>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    var result = geocodeData?.FirstOrDefault();
                    if (result != null && double.TryParse(result.Lat, out var lat) && double.TryParse(result.Lon, out var lon))
                    {
                        _logger.LogInformation("Geocoded successfully: {Lat}, {Lon}", lat, lon);
                        return Tuple.Create((double?)lat, (double?)lon);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to parse geocoding result");
                    }
                }
                else
                {
                    _logger.LogWarning("Geocode request failed with status: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error geocoding address: {Address}", address);
            }
            return null;
        }

        private decimal EstimateDistanceFromAddresses(string startAddress, string endAddress)
        {
            if (string.IsNullOrWhiteSpace(startAddress) || string.IsNullOrWhiteSpace(endAddress))
                return 0;
            
            var startCity = ExtractCity(startAddress);
            var endCity = ExtractCity(endAddress);
            
            if (!string.IsNullOrEmpty(startCity) && !string.IsNullOrEmpty(endCity) && startCity != endCity)
            {
                return 25.0m;
            }
            else if (!string.IsNullOrEmpty(startCity) && !string.IsNullOrEmpty(endCity) && startCity == endCity)
            {
                return 8.0m;
            }
            
            return 5.0m;
        }

        private string? ExtractCity(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return null;
                
            var cities = new[] { "Manila", "Quezon City", "Makati", "Pasay", "Pasig", "Mandaluyong", "San Juan", "Caloocan", "Malabon", "Navotas", "Valenzuela", "Marikina", "Paranaque", "Las Pinas", "Muntinlupa", "Taguig", "Pateros", "Davao City" };
            
            foreach (var city in cities)
            {
                if (address.Contains(city, StringComparison.OrdinalIgnoreCase))
                    return city;
            }
            
            return null;
        }

        // Temporary action to add missing archive columns
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddArchiveColumns(int vehicleId)
        {
            try
            {
                // Add IsArchived column
                await _context.Database.ExecuteSqlRawAsync(
                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RouteTrips' AND COLUMN_NAME = 'IsArchived')
                      ALTER TABLE RouteTrips ADD IsArchived BIT NOT NULL DEFAULT 0");

                // Add ArchivedAt column  
                await _context.Database.ExecuteSqlRawAsync(
                    @"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'RouteTrips' AND COLUMN_NAME = 'ArchivedAt')
                      ALTER TABLE RouteTrips ADD ArchivedAt DATETIME2 NULL");

                TempData["Success"] = "Archive columns added successfully!";
                _logger.LogInformation("Archive columns added to RouteTrips table");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding archive columns");
                TempData["Error"] = "Error adding archive columns: " + ex.Message;
            }

            return RedirectToAction(nameof(RoutePage), new { id = vehicleId });
        }
    }

    public class CalculateDistanceRequest
    {
        public string StartAddress { get; set; } = string.Empty;
        public string EndAddress { get; set; } = string.Empty;
    }

    public class OSRMResponse
    {
        public List<OSRMRoute>? Routes { get; set; }
    }

    public class OSRMRoute
    {
        public double? Distance { get; set; }
        public double? Duration { get; set; }
        public OSRMGeometry? Geometry { get; set; }
    }

    public class OSRMGeometry
    {
        public string? Type { get; set; }
        public List<List<double>>? Coordinates { get; set; }
    }

    public class NominatimResponse
    {
        public string? Lat { get; set; }
        public string? Lon { get; set; }
        public string? Display_Name { get; set; }
    }
}
