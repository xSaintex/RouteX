using Microsoft.AspNetCore.Mvc;
using RouteX.Models;

namespace RouteX.Controllers
{
    public class MaintenanceController : Controller
    {
        public IActionResult MaintenancePage()
        {
            ViewData["Title"] = "Maintenance Schedule";

            var schedules = new List<MaintenanceEntry>
            {
                new MaintenanceEntry { MaintenanceId = 101, PlateNumber = "TRK-001", ServiceType = "Oil Change", ServiceDate = new DateTime(2025, 12, 05), Cost = 120.50m, TechnicianName = "J. Santos", Status = MaintenanceStatus.Pending },
                new MaintenanceEntry { MaintenanceId = 102, PlateNumber = "VAN-002", ServiceType = "Brake Inspection", ServiceDate = new DateTime(2025, 12, 07), Cost = 210.00m, TechnicianName = "L. Reyes", Status = MaintenanceStatus.Ongoing },
                new MaintenanceEntry { MaintenanceId = 103, PlateNumber = "TRK-003", ServiceType = "Tire Rotation", ServiceDate = new DateTime(2025, 12, 02), Cost = 85.75m, TechnicianName = "A. Garcia", Status = MaintenanceStatus.Completed },
                new MaintenanceEntry { MaintenanceId = 104, PlateNumber = "CAR-004", ServiceType = "Engine Diagnostics", ServiceDate = new DateTime(2025, 11, 28), Cost = 320.40m, TechnicianName = "M. Dela Cruz", Status = MaintenanceStatus.Overdue },
                new MaintenanceEntry { MaintenanceId = 105, PlateNumber = "TRK-005", ServiceType = "Transmission Check", ServiceDate = new DateTime(2025, 12, 10), Cost = 260.00m, TechnicianName = "R. Navarro", Status = MaintenanceStatus.Pending }
            };

            return View(schedules);
        }

        // GET: Maintenance/AddMaintenance
        public IActionResult AddMaintenance()
        {
            ViewData["Title"] = "Add Maintenance";
            
            // Get all vehicles from vehicle table list, exclude maintenance vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out maintenance vehicles
            var availableVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Maintenance).ToList();
            
            ViewBag.Vehicles = availableVehicles;
            return View();
        }

        // POST: Maintenance/AddMaintenance
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddMaintenance(MaintenanceEntry maintenanceEntry)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would save to database here
                // For now, we'll just redirect back to MaintenancePage with success message
                TempData["Success"] = "Maintenance record added successfully!";
                return RedirectToAction(nameof(MaintenancePage));
            }
            
            // If validation fails, reload vehicles and return to view
            // Get all vehicles from vehicle table list, exclude maintenance vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out maintenance vehicles
            var availableVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Maintenance).ToList();
            
            ViewBag.Vehicles = availableVehicles;
            return View(maintenanceEntry);
        }

        // GET: Maintenance/EditMaintenance/5
        public IActionResult EditMaintenance(int id)
        {
            ViewData["Title"] = "Edit Maintenance";
            
            // Get all maintenance entries (same data as MaintenancePage)
            var allMaintenanceEntries = new List<MaintenanceEntry>
            {
                new MaintenanceEntry { Id = 101, MaintenanceId = 101, VehicleId = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", ServiceType = "Oil Change", ServiceDate = new DateTime(2025, 12, 05), Cost = 120.50m, TechnicianName = "J. Santos", Status = MaintenanceStatus.Pending, OdometerAtService = 15000, NextServiceDue = new DateTime(2026, 03, 05), Description = "Regular oil change service completed. Used synthetic oil for better engine performance." },
                new MaintenanceEntry { Id = 102, MaintenanceId = 102, VehicleId = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", ServiceType = "Brake Inspection", ServiceDate = new DateTime(2025, 12, 07), Cost = 210.00m, TechnicianName = "L. Reyes", Status = MaintenanceStatus.Ongoing, OdometerAtService = 28500, NextServiceDue = new DateTime(2025, 12, 14), Description = "Front brake pads replacement in progress. Rear pads at 60% life." },
                new MaintenanceEntry { Id = 103, MaintenanceId = 103, VehicleId = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", ServiceType = "Tire Rotation", ServiceDate = new DateTime(2025, 12, 02), Cost = 85.75m, TechnicianName = "A. Garcia", Status = MaintenanceStatus.Completed, OdometerAtService = 41800, NextServiceDue = new DateTime(2026, 03, 02), Description = "Tire rotation completed. All tires showing even wear patterns." },
                new MaintenanceEntry { Id = 104, MaintenanceId = 104, VehicleId = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", ServiceType = "Engine Diagnostics", ServiceDate = new DateTime(2025, 11, 28), Cost = 320.40m, TechnicianName = "M. Dela Cruz", Status = MaintenanceStatus.Overdue, OdometerAtService = 15200, NextServiceDue = new DateTime(2025, 11, 30), Description = "Engine diagnostic revealed sensor issue. Parts ordered for repair." },
                new MaintenanceEntry { Id = 105, MaintenanceId = 105, VehicleId = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", ServiceType = "Transmission Check", ServiceDate = new DateTime(2025, 12, 10), Cost = 260.00m, TechnicianName = "R. Navarro", Status = MaintenanceStatus.Pending, OdometerAtService = 52800, NextServiceDue = new DateTime(2026, 03, 10), Description = "Scheduled transmission fluid check and filter replacement." }
            };
            
            // Find the specific maintenance entry by ID
            var maintenanceEntry = allMaintenanceEntries.FirstOrDefault(m => m.Id == id);
            
            // If not found, create a default entry
            if (maintenanceEntry == null)
            {
                maintenanceEntry = new MaintenanceEntry
                {
                    Id = id,
                    VehicleId = 1,
                    PlateNumber = "TRK-001",
                    UnitModel = "Isuzu N-Series",
                    ServiceType = "Unknown",
                    ServiceDate = DateTime.Now,
                    Cost = 0,
                    TechnicianName = "Unknown",
                    Status = MaintenanceStatus.Pending,
                    OdometerAtService = 0,
                    NextServiceDue = DateTime.Now.AddMonths(3),
                    Description = "Entry not found"
                };
            }
            
            // Get all vehicles from vehicle table list, exclude maintenance vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out maintenance vehicles
            var availableVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Maintenance).ToList();
            
            ViewBag.Vehicles = availableVehicles;
            return View(maintenanceEntry);
        }

        // POST: Maintenance/EditMaintenance/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditMaintenance(MaintenanceEntry maintenanceEntry)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would update the database here
                // For now, we'll just redirect back to MaintenancePage with success message
                TempData["Success"] = "Maintenance record updated successfully!";
                return RedirectToAction(nameof(MaintenancePage));
            }
            
            // If validation fails, reload vehicles and return to view
            // Get all vehicles from vehicle table list, exclude maintenance vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out maintenance vehicles
            var availableVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Maintenance).ToList();
            
            ViewBag.Vehicles = availableVehicles;
            return View(maintenanceEntry);
        }

        // GET: Maintenance/ViewMaintenance/5
        public IActionResult ViewMaintenance(int id)
        {
            ViewData["Title"] = "View Maintenance";
            
            // Get all maintenance entries (same data as MaintenancePage and EditMaintenance)
            var allMaintenanceEntries = new List<MaintenanceEntry>
            {
                new MaintenanceEntry { Id = 101, MaintenanceId = 101, VehicleId = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", ServiceType = "Oil Change", ServiceDate = new DateTime(2025, 12, 05), Cost = 120.50m, TechnicianName = "J. Santos", Status = MaintenanceStatus.Pending, OdometerAtService = 15000, NextServiceDue = new DateTime(2026, 03, 05), Description = "Regular oil change service completed. Used synthetic oil for better engine performance." },
                new MaintenanceEntry { Id = 102, MaintenanceId = 102, VehicleId = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", ServiceType = "Brake Inspection", ServiceDate = new DateTime(2025, 12, 07), Cost = 210.00m, TechnicianName = "L. Reyes", Status = MaintenanceStatus.Ongoing, OdometerAtService = 28500, NextServiceDue = new DateTime(2025, 12, 14), Description = "Front brake pads replacement in progress. Rear pads at 60% life." },
                new MaintenanceEntry { Id = 103, MaintenanceId = 103, VehicleId = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", ServiceType = "Tire Rotation", ServiceDate = new DateTime(2025, 12, 02), Cost = 85.75m, TechnicianName = "A. Garcia", Status = MaintenanceStatus.Completed, OdometerAtService = 41800, NextServiceDue = new DateTime(2026, 03, 02), Description = "Tire rotation completed. All tires showing even wear patterns." },
                new MaintenanceEntry { Id = 104, MaintenanceId = 104, VehicleId = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", ServiceType = "Engine Diagnostics", ServiceDate = new DateTime(2025, 11, 28), Cost = 320.40m, TechnicianName = "M. Dela Cruz", Status = MaintenanceStatus.Overdue, OdometerAtService = 15200, NextServiceDue = new DateTime(2025, 11, 30), Description = "Engine diagnostic revealed sensor issue. Parts ordered for repair." },
                new MaintenanceEntry { Id = 105, MaintenanceId = 105, VehicleId = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", ServiceType = "Transmission Check", ServiceDate = new DateTime(2025, 12, 10), Cost = 260.00m, TechnicianName = "R. Navarro", Status = MaintenanceStatus.Pending, OdometerAtService = 52800, NextServiceDue = new DateTime(2026, 03, 10), Description = "Scheduled transmission fluid check and filter replacement." }
            };
            
            // Find the specific maintenance entry by ID
            var maintenanceEntry = allMaintenanceEntries.FirstOrDefault(m => m.Id == id);
            
            // If not found, create a default entry
            if (maintenanceEntry == null)
            {
                maintenanceEntry = new MaintenanceEntry
                {
                    Id = id,
                    VehicleId = 1,
                    PlateNumber = "TRK-001",
                    UnitModel = "Isuzu N-Series",
                    ServiceType = "Unknown",
                    ServiceDate = DateTime.Now,
                    Cost = 0,
                    TechnicianName = "Unknown",
                    Status = MaintenanceStatus.Pending,
                    OdometerAtService = 0,
                    NextServiceDue = DateTime.Now.AddMonths(3),
                    Description = "Entry not found"
                };
            }
            
            // Get all vehicles from vehicle table list, exclude maintenance vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out maintenance vehicles
            var availableVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Maintenance).ToList();
            
            ViewBag.Vehicles = availableVehicles;
            return View(maintenanceEntry);
        }
    }
}
