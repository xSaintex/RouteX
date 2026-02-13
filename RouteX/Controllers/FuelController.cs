using Microsoft.AspNetCore.Mvc;
using RouteX.Models;

namespace RouteX.Controllers
{
    public class FuelController : Controller
    {
        public IActionResult FuelPage()
        {
            ViewData["Title"] = "Fuel Management";

            var fuelEntries = new List<FuelEntry>
            {
                new FuelEntry { Id = 1, UnitModel = "Isuzu N-Series", PlateNumber = "TRK-001", Date = new DateTime(2025, 12, 10), Liters = 120.5m, TotalCost = 185.75m, FuelStation = "Shell", Driver = "John Smith" },
                new FuelEntry { Id = 2, UnitModel = "Toyota HiAce", PlateNumber = "VAN-002", Date = new DateTime(2025, 12, 12), Liters = 78.0m, TotalCost = 120.10m, FuelStation = "Petron", Driver = "Maria Garcia" },
                new FuelEntry { Id = 3, UnitModel = "Hino 300", PlateNumber = "TRK-003", Date = new DateTime(2025, 12, 14), Liters = 95.4m, TotalCost = 150.40m, FuelStation = "Caltex", Driver = "Robert Chen" },
                new FuelEntry { Id = 4, UnitModel = "Toyota Corolla", PlateNumber = "CAR-004", Date = new DateTime(2025, 12, 15), Liters = 45.2m, TotalCost = 70.25m, FuelStation = "Shell", Driver = "Sarah Johnson" },
                new FuelEntry { Id = 5, UnitModel = "Mitsubishi Canter", PlateNumber = "TRK-005", Date = new DateTime(2025, 12, 17), Liters = 110.8m, TotalCost = 172.90m, FuelStation = "Petron", Driver = "David Wilson" }
            };

            return View(fuelEntries);
        }

        // GET: Fuel/AddFuel
        public IActionResult AddFuel()
        {
            ViewData["Title"] = "Add Fuel";
            
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View();
        }

        // POST: Fuel/AddFuel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFuel(FuelEntry fuelEntry, IFormFile receiptUpload)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would save to database here
                // For now, we'll just redirect back to FuelPage with success message
                TempData["Success"] = "Fuel record added successfully!";
                return RedirectToAction(nameof(FuelPage));
            }
            
            return View(fuelEntry);
        }

        // GET: Fuel/EditFuel/5
        public IActionResult EditFuel(int id)
        {
            ViewData["Title"] = "Edit Fuel";
            
            // Get all fuel entries (same data as FuelPage)
            var allFuelEntries = new List<FuelEntry>
            {
                new FuelEntry { Id = 1, VehicleId = 1, UnitModel = "Isuzu N-Series", PlateNumber = "TRK-001", Date = new DateTime(2025, 12, 10), DateTime = new DateTime(2025, 12, 10, 14, 30, 0), Liters = 120.5m, TotalCost = 185.75m, FuelStation = "Shell", Driver = "John Smith", FuelType = "Diesel", FullTank = true, Odometer = 15420, Notes = "Regular oil change service" },
                new FuelEntry { Id = 2, VehicleId = 2, UnitModel = "Toyota HiAce", PlateNumber = "VAN-002", Date = new DateTime(2025, 12, 12), DateTime = new DateTime(2025, 12, 12, 09, 15, 0), Liters = 78.0m, TotalCost = 120.10m, FuelStation = "Petron", Driver = "Maria Garcia", FuelType = "Premium", FullTank = true, Odometer = 28900, Notes = "Long distance trip fuel" },
                new FuelEntry { Id = 3, VehicleId = 3, UnitModel = "Hino 300", PlateNumber = "TRK-003", Date = new DateTime(2025, 12, 14), DateTime = new DateTime(2025, 12, 14, 11, 45, 0), Liters = 95.4m, TotalCost = 150.40m, FuelStation = "Caltex", Driver = "Robert Chen", FuelType = "Diesel", FullTank = false, Odometer = 42100, Notes = "Partial fill for city driving" },
                new FuelEntry { Id = 4, VehicleId = 4, UnitModel = "Toyota Corolla", PlateNumber = "CAR-004", Date = new DateTime(2025, 12, 15), DateTime = new DateTime(2025, 12, 15, 16, 20, 0), Liters = 45.2m, TotalCost = 70.25m, FuelStation = "Shell", Driver = "Sarah Johnson", FuelType = "Regular", FullTank = true, Odometer = 15600, Notes = "Weekly commute fuel" },
                new FuelEntry { Id = 5, VehicleId = 5, UnitModel = "Mitsubishi Canter", PlateNumber = "TRK-005", Date = new DateTime(2025, 12, 17), DateTime = new DateTime(2025, 12, 17, 13, 10, 0), Liters = 110.8m, TotalCost = 172.90m, FuelStation = "Petron", Driver = "David Wilson", FuelType = "Diesel", FullTank = true, Odometer = 53400, Notes = "Heavy load delivery preparation" }
            };
            
            // Find the specific fuel entry by ID
            var fuelEntry = allFuelEntries.FirstOrDefault(f => f.Id == id);
            
            // If not found, create a default entry or return not found
            if (fuelEntry == null)
            {
                fuelEntry = new FuelEntry
                {
                    Id = id,
                    VehicleId = 1,
                    UnitModel = "Isuzu N-Series",
                    PlateNumber = "TRK-001",
                    Driver = "Unknown",
                    DateTime = DateTime.Now,
                    FuelStation = "Unknown",
                    Odometer = 0,
                    Liters = 0,
                    TotalCost = 0,
                    FuelType = "Diesel",
                    FullTank = false,
                    Notes = "Entry not found"
                };
            }
            
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(fuelEntry);
        }

        // POST: Fuel/EditFuel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFuel(FuelEntry fuelEntry, IFormFile receiptUpload)
        {
            if (ModelState.IsValid)
            {
                // In a real application, you would update the database here
                // For now, we'll just redirect back to FuelPage with success message
                TempData["Success"] = "Fuel record updated successfully!";
                return RedirectToAction(nameof(FuelPage));
            }
            
            // If validation fails, reload vehicles and return to view
            // Get all vehicles from vehicle table list, exclude inactive vehicles
            var allVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 15420 },
                new Vehicle { Id = 2, PlateNumber = "VAN-002", UnitModel = "Toyota HiAce", VehicleType = "Van", Status = VehicleStatus.Maintenance, Mileage = 28950 },
                new Vehicle { Id = 3, PlateNumber = "TRK-003", UnitModel = "Hino 300", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 8760 },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", VehicleType = "Car", Status = VehicleStatus.Inactive, Mileage = 45230 },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", VehicleType = "Truck", Status = VehicleStatus.Active, Mileage = 12340 }
            };
            
            // Filter out inactive vehicles
            var activeVehicles = allVehicles.Where(v => v.Status != VehicleStatus.Inactive).ToList();
            
            ViewBag.Vehicles = activeVehicles;
            return View(fuelEntry);
        }

        // GET: Fuel/ViewFuel/5
        public IActionResult ViewFuel(int id)
        {
            ViewData["Title"] = "View Fuel";
            
            // Get all fuel entries (same data as FuelPage and EditFuel)
            var allFuelEntries = new List<FuelEntry>
            {
                new FuelEntry { Id = 1, VehicleId = 1, UnitModel = "Isuzu N-Series", PlateNumber = "TRK-001", Date = new DateTime(2025, 12, 10), DateTime = new DateTime(2025, 12, 10, 14, 30, 0), Liters = 120.5m, TotalCost = 185.75m, FuelStation = "Shell", Driver = "John Smith", FuelType = "Diesel", FullTank = true, Odometer = 15420, Notes = "Regular oil change service" },
                new FuelEntry { Id = 2, VehicleId = 2, UnitModel = "Toyota HiAce", PlateNumber = "VAN-002", Date = new DateTime(2025, 12, 12), DateTime = new DateTime(2025, 12, 12, 09, 15, 0), Liters = 78.0m, TotalCost = 120.10m, FuelStation = "Petron", Driver = "Maria Garcia", FuelType = "Premium", FullTank = true, Odometer = 28900, Notes = "Long distance trip fuel" },
                new FuelEntry { Id = 3, VehicleId = 3, UnitModel = "Hino 300", PlateNumber = "TRK-003", Date = new DateTime(2025, 12, 14), DateTime = new DateTime(2025, 12, 14, 11, 45, 0), Liters = 95.4m, TotalCost = 150.40m, FuelStation = "Caltex", Driver = "Robert Chen", FuelType = "Diesel", FullTank = false, Odometer = 42100, Notes = "Partial fill for city driving" },
                new FuelEntry { Id = 4, VehicleId = 4, UnitModel = "Toyota Corolla", PlateNumber = "CAR-004", Date = new DateTime(2025, 12, 15), DateTime = new DateTime(2025, 12, 15, 16, 20, 0), Liters = 45.2m, TotalCost = 70.25m, FuelStation = "Shell", Driver = "Sarah Johnson", FuelType = "Regular", FullTank = true, Odometer = 15600, Notes = "Weekly commute fuel" },
                new FuelEntry { Id = 5, VehicleId = 5, UnitModel = "Mitsubishi Canter", PlateNumber = "TRK-005", Date = new DateTime(2025, 12, 17), DateTime = new DateTime(2025, 12, 17, 13, 10, 0), Liters = 110.8m, TotalCost = 172.90m, FuelStation = "Petron", Driver = "David Wilson", FuelType = "Diesel", FullTank = true, Odometer = 53400, Notes = "Heavy load delivery preparation" }
            };
            
            // Find the specific fuel entry by ID
            var fuelEntry = allFuelEntries.FirstOrDefault(f => f.Id == id);
            
            // If not found, create a default entry
            if (fuelEntry == null)
            {
                fuelEntry = new FuelEntry
                {
                    Id = id,
                    VehicleId = 1,
                    UnitModel = "Isuzu N-Series",
                    PlateNumber = "TRK-001",
                    Driver = "Unknown",
                    DateTime = DateTime.Now,
                    FuelStation = "Unknown",
                    Odometer = 0,
                    Liters = 0,
                    TotalCost = 0,
                    FuelType = "Diesel",
                    FullTank = false,
                    Notes = "Entry not found"
                };
            }
            
            // Get active vehicles for dropdown display
            var activeVehicles = new List<Vehicle>
            {
                new Vehicle { Id = 1, PlateNumber = "TRK-001", UnitModel = "Isuzu N-Series", Status = VehicleStatus.Active },
                new Vehicle { Id = 4, PlateNumber = "CAR-004", UnitModel = "Toyota Corolla", Status = VehicleStatus.Active },
                new Vehicle { Id = 5, PlateNumber = "TRK-005", UnitModel = "Mitsubishi Canter", Status = VehicleStatus.Active }
            };
            
            ViewBag.Vehicles = activeVehicles;
            return View(fuelEntry);
        }
    }
}
