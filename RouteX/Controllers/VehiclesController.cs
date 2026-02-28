using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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

        public VehiclesController(ApplicationDbContext context, IAuditService auditService, IConfiguration configuration)
        {
            _context = context;
            _auditService = auditService;
            _configuration = configuration;
        }

        public async Task<IActionResult> VehiclePage(string? status)
        {
            ViewData["Title"] = "Vehicle Management";

            var query = _context.Vehicles
                .AsNoTracking()
                .Where(v => !v.IsArchived);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VehicleStatus>(status, true, out var parsedStatus))
            {
                query = query.Where(v => v.Status == parsedStatus);
                ViewBag.InitialStatusFilter = parsedStatus.ToString();
            }

            var vehicles = await query
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            return View(vehicles);
        }

        // GET: Vehicles/AddVehicle
        public IActionResult AddVehicle()
        {
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

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                var actingUser = HttpContext.Session.GetString("UserEmail") ?? "System";
                await _auditService.LogActionAsync(actingUser, $"Create:Vehicle:{vehicle.Id}");
                TempData["Success"] = "Vehicle added successfully!";
                TempData["RecentVehicleId"] = vehicle.Id; // Track recently added vehicle
                TempData["RecentAction"] = "Added"; // Track action type
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
                existingVehicle.Mileage = vehicle.Mileage;

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
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
            {
                return NotFound();
            }

            var currentRoute = await _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.VehicleId == id && r.Status == RouteTripStatus.Saved)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            var tripHistory = await _context.RouteTrips
                .AsNoTracking()
                .Where(r => r.VehicleId == id && r.Status == RouteTripStatus.Completed)
                .OrderByDescending(r => r.CompletedAt ?? r.CreatedAt)
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

            route.Status = RouteTripStatus.Completed;
            route.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Route marked as completed.";
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
    }
}
