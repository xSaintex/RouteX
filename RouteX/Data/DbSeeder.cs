using Microsoft.EntityFrameworkCore;
using RouteX.Models;

namespace RouteX.Data
{
    public class DbSeeder
    {
        public static async Task SeedBranches(ApplicationDbContext context)
        {
            // Check if branches already exist
            if (await context.Branches.AnyAsync())
            {
                return; // Database has been seeded
            }

            // Create default branches
            var branches = new[]
            {
                new Branch
                {
                    BranchName = "Main Headquarters",
                    Address = "123 Main Street",
                    City = "Manila",
                    Province = "Metro Manila",
                    PostalCode = "1000",
                    PhoneNumber = "+63-2-1234-5678",
                    Email = "main@routex.com",
                    Latitude = 14.5995m,
                    Longitude = 120.9842m,
                    CoverageRadiusKm = 50.0m,
                    Status = BranchStatus.Active, // Active
                    ManagerName = "System Administrator",
                    OperatingHours = "24/7",
                    ServiceAreas = "Metro Manila and surrounding provinces",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "North Branch",
                    Address = "456 North Avenue",
                    City = "Quezon City",
                    Province = "Metro Manila",
                    PostalCode = "1100",
                    PhoneNumber = "+63-2-2345-6789",
                    Email = "north@routex.com",
                    Latitude = 14.6760m,
                    Longitude = 121.0437m,
                    CoverageRadiusKm = 30.0m,
                    Status = BranchStatus.Active, // Active
                    ManagerName = "North Branch Manager",
                    OperatingHours = "6:00 AM - 10:00 PM",
                    ServiceAreas = "Quezon City, Caloocan, Malabon",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "South Branch",
                    Address = "789 South Boulevard",
                    City = "Makati",
                    Province = "Metro Manila",
                    PostalCode = "1200",
                    PhoneNumber = "+63-2-3456-7890",
                    Email = "south@routex.com",
                    Latitude = 14.5547m,
                    Longitude = 121.0244m,
                    CoverageRadiusKm = 25.0m,
                    Status = BranchStatus.Active, // Active
                    ManagerName = "South Branch Manager",
                    OperatingHours = "7:00 AM - 9:00 PM",
                    ServiceAreas = "Makati, Pasay, Taguig",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "System",
                    UpdatedBy = "System",
                    IsArchived = false
                }
            };

            await context.Branches.AddRangeAsync(branches);
            await context.SaveChangesAsync();
        }
    }
}
