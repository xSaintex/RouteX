using Microsoft.EntityFrameworkCore;
using RouteX.Models;

namespace RouteX.Data
{
    public static class BranchSeedData
    {
        public static void SeedBranches(ApplicationDbContext context)
        {
            // Check if branches already exist
            if (context.Branches.Any())
            {
                return; // Data already seeded
            }

            var branches = new List<Branch>
            {
                // Metro Manila Branches
                new Branch
                {
                    BranchName = "RouteX Main Headquarters",
                    Address = "123 EDSA Boulevard",
                    City = "Makati City",
                    Province = "Metro Manila",
                    PostalCode = "1200",
                    PhoneNumber = "+63 2 8888 1001",
                    Email = "headquarters@routex.com",
                    Latitude = 14.5547m,
                    Longitude = 121.0244m,
                    CoverageRadiusKm = 25.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Juan Dela Cruz",
                    OperatingHours = "Mon-Sun: 6:00 AM - 10:00 PM",
                    ServiceAreas = "Makati, Taguig, BGC, Pasay, Paranaque",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Quezon City Hub",
                    Address = "456 Quezon Avenue",
                    City = "Quezon City",
                    Province = "Metro Manila",
                    PostalCode = "1100",
                    PhoneNumber = "+63 2 8888 1002",
                    Email = "qc@routex.com",
                    Latitude = 14.6488m,
                    Longitude = 121.0509m,
                    CoverageRadiusKm = 20.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Maria Santos",
                    OperatingHours = "Mon-Sun: 6:00 AM - 10:00 PM",
                    ServiceAreas = "Quezon City, Caloocan, Valenzuela, San Juan",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Manila Central",
                    Address = "789 Taft Avenue",
                    City = "Manila",
                    Province = "Metro Manila",
                    PostalCode = "1000",
                    PhoneNumber = "+63 2 8888 1003",
                    Email = "manila@routex.com",
                    Latitude = 14.5995m,
                    Longitude = 120.9842m,
                    CoverageRadiusKm = 15.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Pedro Garcia",
                    OperatingHours = "Mon-Sun: 5:00 AM - 11:00 PM",
                    ServiceAreas = "Manila, Sampaloc, Ermita, Malate",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Pasig Station",
                    Address = "321 Ortigas Center",
                    City = "Pasig City",
                    Province = "Metro Manila",
                    PostalCode = "1605",
                    PhoneNumber = "+63 2 8888 1004",
                    Email = "pasig@routex.com",
                    Latitude = 14.5876m,
                    Longitude = 121.0615m,
                    CoverageRadiusKm = 18.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Ana Reyes",
                    OperatingHours = "Mon-Sun: 6:00 AM - 10:00 PM",
                    ServiceAreas = "Pasig, Mandaluyong, Marikina, Cainta",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },

                // Luzon Branches
                new Branch
                {
                    BranchName = "RouteX Cavite South",
                    Address = "567 Aguinaldo Highway",
                    City = "Imus",
                    Province = "Cavite",
                    PostalCode = "4103",
                    PhoneNumber = "+63 46 471 1005",
                    Email = "cavite@routex.com",
                    Latitude = 14.4297m,
                    Longitude = 120.9367m,
                    CoverageRadiusKm = 30.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Roberto Mendoza",
                    OperatingHours = "Mon-Sat: 6:00 AM - 9:00 PM",
                    ServiceAreas = "Imus, Bacoor, Dasmarinas, General Trias",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Laguna Station",
                    Address = "890 National Highway",
                    City = "Santa Rosa",
                    Province = "Laguna",
                    PostalCode = "4026",
                    PhoneNumber = "+63 49 534 1006",
                    Email = "laguna@routex.com",
                    Latitude = 14.3108m,
                    Longitude = 121.1114m,
                    CoverageRadiusKm = 35.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Carmen Villanueva",
                    OperatingHours = "Mon-Sat: 6:00 AM - 9:00 PM",
                    ServiceAreas = "Santa Rosa, Binan, Calamba, San Pedro",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Bulacan North",
                    Address = "234 McArthur Highway",
                    City = "Malolos",
                    Province = "Bulacan",
                    PostalCode = "3000",
                    PhoneNumber = "+63 44 791 1007",
                    Email = "bulacan@routex.com",
                    Latitude = 14.8527m,
                    Longitude = 120.8100m,
                    CoverageRadiusKm = 40.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Eduardo Torres",
                    OperatingHours = "Mon-Sat: 5:00 AM - 8:00 PM",
                    ServiceAreas = "Malolos, Meycauayan, San Jose del Monte, Bocaue",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Pampanga Hub",
                    Address = "456 MacArthur Highway",
                    City = "San Fernando",
                    Province = "Pampanga",
                    PostalCode = "2000",
                    PhoneNumber = "+63 45 961 1008",
                    Email = "pampanga@routex.com",
                    Latitude = 15.0286m,
                    Longitude = 120.6872m,
                    CoverageRadiusKm = 45.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Gloria Pangilinan",
                    OperatingHours = "Mon-Sat: 5:00 AM - 8:00 PM",
                    ServiceAreas = "San Fernando, Angeles City, Guagua, Lubao",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Batangas Port",
                    Address = "789 Diversion Road",
                    City = "Batangas City",
                    Province = "Batangas",
                    PostalCode = "4200",
                    PhoneNumber = "+63 43 723 1009",
                    Email = "batangas@routex.com",
                    Latitude = 13.7565m,
                    Longitude = 121.0583m,
                    CoverageRadiusKm = 50.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Ricardo Dimaculangan",
                    OperatingHours = "Mon-Sun: 24 Hours",
                    ServiceAreas = "Batangas City, Lipa, Tanauan, Santo Tomas",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Baguio Mountain",
                    Address = "123 Session Road",
                    City = "Baguio City",
                    Province = "Benguet",
                    PostalCode = "2600",
                    PhoneNumber = "+63 74 442 1010",
                    Email = "baguio@routex.com",
                    Latitude = 16.4023m,
                    Longitude = 120.5960m,
                    CoverageRadiusKm = 30.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Benedicto Carino",
                    OperatingHours = "Mon-Sat: 6:00 AM - 8:00 PM",
                    ServiceAreas = "Baguio City, La Trinidad, Itogon, Tuba",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },

                // Visayas Branches
                new Branch
                {
                    BranchName = "RouteX Cebu Central",
                    Address = "456 Osmena Boulevard",
                    City = "Cebu City",
                    Province = "Cebu",
                    PostalCode = "6000",
                    PhoneNumber = "+63 32 253 1011",
                    Email = "cebu@routex.com",
                    Latitude = 10.3157m,
                    Longitude = 123.8854m,
                    CoverageRadiusKm = 35.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Lourdes Gonzales",
                    OperatingHours = "Mon-Sun: 6:00 AM - 10:00 PM",
                    ServiceAreas = "Cebu City, Mandaue, Lapu-Lapu, Talisay",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Iloilo West Visayas",
                    Address = "789 Diversion Road",
                    City = "Iloilo City",
                    Province = "Iloilo",
                    PostalCode = "5000",
                    PhoneNumber = "+63 33 337 1012",
                    Email = "iloilo@routex.com",
                    Latitude = 10.7202m,
                    Longitude = 122.5621m,
                    CoverageRadiusKm = 40.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Francisco Treñas",
                    OperatingHours = "Mon-Sat: 6:00 AM - 9:00 PM",
                    ServiceAreas = "Iloilo City, Pavia, Santa Barbara, Oton",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Bacolod Sugar",
                    Address = "321 Lacson Street",
                    City = "Bacolod City",
                    Province = "Negros Occidental",
                    PostalCode = "6100",
                    PhoneNumber = "+63 34 433 1013",
                    Email = "bacolod@routex.com",
                    Latitude = 10.6713m,
                    Longitude = 122.9511m,
                    CoverageRadiusKm = 35.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Marissa Leonardia",
                    OperatingHours = "Mon-Sat: 6:00 AM - 9:00 PM",
                    ServiceAreas = "Bacolod City, Talisay, Silay, Victorias",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Tacloban Eastern",
                    Address = "567 Real Street",
                    City = "Tacloban City",
                    Province = "Leyte",
                    PostalCode = "6500",
                    PhoneNumber = "+63 53 321 1014",
                    Email = "tacloban@routex.com",
                    Latitude = 11.2543m,
                    Longitude = 124.9632m,
                    CoverageRadiusKm = 45.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Alfred Romualdez",
                    OperatingHours = "Mon-Sat: 6:00 AM - 8:00 PM",
                    ServiceAreas = "Tacloban City, Palo, Tanauan, Tolosa",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },

                // Mindanao Branches
                new Branch
                {
                    BranchName = "RouteX Davao South",
                    Address = "890 JP Laurel Avenue",
                    City = "Davao City",
                    Province = "Davao del Sur",
                    PostalCode = "8000",
                    PhoneNumber = "+63 82 227 1015",
                    Email = "davao@routex.com",
                    Latitude = 7.0731m,
                    Longitude = 125.6128m,
                    CoverageRadiusKm = 50.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Sebastian Duterte",
                    OperatingHours = "Mon-Sun: 6:00 AM - 10:00 PM",
                    ServiceAreas = "Davao City, Tagum, Panabo, Digos",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX CDO Northern Mindanao",
                    Address = "123 Divisoria Street",
                    City = "Cagayan de Oro City",
                    Province = "Misamis Oriental",
                    PostalCode = "9000",
                    PhoneNumber = "+63 88 857 1016",
                    Email = "cdo@routex.com",
                    Latitude = 8.4542m,
                    Longitude = 124.6319m,
                    CoverageRadiusKm = 45.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Oscar Moreno",
                    OperatingHours = "Mon-Sat: 6:00 AM - 9:00 PM",
                    ServiceAreas = "CDO, Iligan, Valencia, El Salvador",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX General Santos Soccsksargen",
                    Address = "456 National Highway",
                    City = "General Santos City",
                    Province = "South Cotabato",
                    PostalCode = "9500",
                    PhoneNumber = "+63 83 552 1017",
                    Email = "gensan@routex.com",
                    Latitude = 6.1108m,
                    Longitude = 125.1716m,
                    CoverageRadiusKm = 55.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Ronnel Rivera",
                    OperatingHours = "Mon-Sat: 5:00 AM - 8:00 PM",
                    ServiceAreas = "General Santos, Koronadal, Polomolok, Tupi",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Zamboanga Peninsula",
                    Address = "789 Gov. Alvarez Avenue",
                    City = "Zamboanga City",
                    Province = "Zamboanga del Sur",
                    PostalCode = "7000",
                    PhoneNumber = "+63 62 991 1018",
                    Email = "zamboanga@routex.com",
                    Latitude = 6.9214m,
                    Longitude = 122.0790m,
                    CoverageRadiusKm = 40.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Beng Climaco",
                    OperatingHours = "Mon-Sat: 6:00 AM - 8:00 PM",
                    ServiceAreas = "Zamboanga City, Ipil, Pagadian",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Butuan Caraga",
                    Address = "321 JC Aquino Avenue",
                    City = "Butuan City",
                    Province = "Agusan del Norte",
                    PostalCode = "8600",
                    PhoneNumber = "+63 85 341 1019",
                    Email = "butuan@routex.com",
                    Latitude = 8.9475m,
                    Longitude = 125.5406m,
                    CoverageRadiusKm = 50.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Ronnie Lagnada",
                    OperatingHours = "Mon-Sat: 6:00 AM - 8:00 PM",
                    ServiceAreas = "Butuan City, Cabadbaran, Nasipit, Bayugan",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                },
                new Branch
                {
                    BranchName = "RouteX Cotabato BARMM",
                    Address = "567 Sinsuat Avenue",
                    City = "Cotabato City",
                    Province = "Maguindanao",
                    PostalCode = "9600",
                    PhoneNumber = "+63 64 421 1020",
                    Email = "cotabato@routex.com",
                    Latitude = 7.2047m,
                    Longitude = 124.2310m,
                    CoverageRadiusKm = 60.00m,
                    Status = BranchStatus.Active,
                    ManagerName = "Mohammad Sema",
                    OperatingHours = "Mon-Sat: 6:00 AM - 7:00 PM",
                    ServiceAreas = "Cotabato City, Sultan Kudarat, Kidapawan",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = "SuperAdmin",
                    IsArchived = false
                }
            };

            context.Branches.AddRange(branches);
            context.SaveChanges();
        }
    }
}
