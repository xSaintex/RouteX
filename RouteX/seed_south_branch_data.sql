-- Seed South Branch (BranchId = 3) with 50 vehicles and trip history

-- Insert 20 Active Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, Status, TotalMileage, IsArchived, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) VALUES
('SBR-1001', 'Toyota Vios', 0, 15420.50, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1002', 'Honda Civic', 0, 23150.75, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1003', 'Mitsubishi Mirage', 0, 8920.25, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1004', 'Nissan Sentra', 0, 31200.00, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1005', 'Ford Ranger', 0, 45600.80, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1006', 'Isuzu D-Max', 0, 28900.60, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1007', 'Toyota Hilux', 0, 51200.90, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1008', 'Honda CR-V', 0, 19800.40, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1009', 'Mitsubishi Xpander', 0, 12300.20, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1010', 'Nissan Navara', 0, 38700.55, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1011', 'Ford Everest', 0, 42100.30, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1012', 'Isuzu Mu-X', 0, 35600.70, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1013', 'Toyota Innova', 0, 27800.85, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1014', 'Honda City', 0, 16500.45, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1015', 'Mitsubishi L300', 0, 52300.95, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1016', 'Nissan Urvan', 0, 48900.65, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1017', 'Ford Transit', 0, 41200.25, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1018', 'Isuzu Crosswind', 0, 36700.80, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1019', 'Toyota Avanza', 0, 21300.35, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-1020', 'Honda Jazz', 0, 14200.15, 0, 3, GETDATE(), GETDATE(), 'System', 'System');

-- Insert 10 Inactive Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, Status, TotalMileage, IsArchived, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) VALUES
('SBR-2001', 'Toyota Corolla', 1, 89000.00, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2002', 'Honda Accord', 1, 76500.50, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2003', 'Mitsubishi Lancer', 1, 92300.75, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2004', 'Nissan Altima', 1, 81400.25, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2005', 'Ford Focus', 1, 67800.80, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2006', 'Isuzu Gemini', 1, 95200.40, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2007', 'Toyota Camry', 1, 88700.60, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2008', 'Honda Prelude', 1, 94600.90, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2009', 'Mitsubishi Galant', 1, 72300.30, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-2010', 'Nissan Maxima', 1, 89500.55, 0, 3, GETDATE(), GETDATE(), 'System', 'System');

-- Insert 20 Maintenance Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, Status, TotalMileage, IsArchived, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy) VALUES
('SBR-3001', 'Toyota Hiace', 2, 67800.45, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3002', 'Honda Mobilio', 2, 54300.70, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3003', 'Mitsubishi Adventure', 2, 78900.85, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3004', 'Nissan Patrol', 2, 91200.20, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3005', 'Ford Expedition', 2, 83400.95, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3006', 'Isuzu Trooper', 2, 75600.15, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3007', 'Toyota Revo', 2, 62300.40, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3008', 'Honda HR-V', 2, 57800.60, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3009', 'Mitsubishi Montero', 2, 84500.80, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3010', 'Nissan X-Trail', 2, 71200.25, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3011', 'Ford Escape', 2, 68900.35, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3012', 'Isuzu Fuego', 2, 73400.50, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3013', 'Toyota Fortuner', 2, 86700.75, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3014', 'Honda BR-V', 2, 59200.90, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3015', 'Mitsubishi Strada', 2, 82300.10, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3016', 'Nissan Terra', 2, 79800.30, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3017', 'Ford Ranger Raptor', 2, 93400.65, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3018', 'Isuzu D-Max', 2, 87600.85, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3019', 'Toyota Land Cruiser', 2, 94500.20, 0, 3, GETDATE(), GETDATE(), 'System', 'System'),
('SBR-3020', 'Honda Pilot', 2, 88900.40, 0, 3, GETDATE(), GETDATE(), 'System', 'System');

-- Get the vehicle IDs for trip history generation
DECLARE @VehicleIds TABLE (Id INT);
INSERT INTO @VehicleIds SELECT Id FROM Vehicles WHERE PlateNumber LIKE 'SBR-%' ORDER BY Id;

-- Generate trip history for all vehicles (3-5 trips per vehicle)
-- Active Vehicles (SBR-1001 to SBR-1020)
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Makati City Hall, Makati',
    'Bonifacio Global City, Taguig',
    ROUND(15 + RAND() * 25, 2),
    2,
    3,
    DATEADD(DAY, -30 + CAST(RAND() * 60 AS INT), GETDATE()),
    DATEADD(DAY, -30 + CAST(RAND() * 60 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -30 + CAST(RAND() * 60 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 51 AND 70;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Ayala Avenue, Makati',
    'Ortigas Center, Pasig',
    ROUND(20 + RAND() * 30, 2),
    2,
    3,
    DATEADD(DAY, -60 + CAST(RAND() * 90 AS INT), GETDATE()),
    DATEADD(DAY, -60 + CAST(RAND() * 90 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -60 + CAST(RAND() * 90 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 51 AND 70;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Makati Medical Center, Makati',
    'St. Luke Medical Center, Quezon City',
    ROUND(25 + RAND() * 35, 2),
    2,
    3,
    DATEADD(DAY, -90 + CAST(RAND() * 120 AS INT), GETDATE()),
    DATEADD(DAY, -90 + CAST(RAND() * 120 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -90 + CAST(RAND() * 120 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 51 AND 70;

-- Inactive Vehicles (SBR-2001 to SBR-2010) - fewer trips
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Makati City Hall, Makati',
    'Manila City Hall, Manila',
    ROUND(18 + RAND() * 22, 2),
    2,
    3,
    DATEADD(DAY, -120 + CAST(RAND() * 150 AS INT), GETDATE()),
    DATEADD(DAY, -120 + CAST(RAND() * 150 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -120 + CAST(RAND() * 150 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 71 AND 80;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Greenbelt Mall, Makati',
    'SM Megamall, Mandaluyong',
    ROUND(22 + RAND() * 28, 2),
    2,
    3,
    DATEADD(DAY, -180 + CAST(RAND() * 200 AS INT), GETDATE()),
    DATEADD(DAY, -180 + CAST(RAND() * 200 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -180 + CAST(RAND() * 200 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 71 AND 80;

-- Maintenance Vehicles (SBR-3001 to SBR-3020) - moderate trips
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Makati Central Business District',
    'Pasay City',
    ROUND(12 + RAND() * 18, 2),
    2,
    3,
    DATEADD(DAY, -45 + CAST(RAND() * 75 AS INT), GETDATE()),
    DATEADD(DAY, -45 + CAST(RAND() * 75 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -45 + CAST(RAND() * 75 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 81 AND 100;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, CompletedAt)
SELECT 
    Id,
    'Makati Avenue, Makati',
    'EDSA, Mandaluyong',
    ROUND(28 + RAND() * 32, 2),
    2,
    3,
    DATEADD(DAY, -75 + CAST(RAND() * 100 AS INT), GETDATE()),
    DATEADD(DAY, -75 + CAST(RAND() * 100 AS INT), GETDATE()),
    'System',
    'System',
    DATEADD(DAY, -75 + CAST(RAND() * 100 AS INT), GETDATE())
FROM @VehicleIds WHERE Id BETWEEN 81 AND 100;

PRINT 'South Branch data seeding completed successfully!';
PRINT '50 vehicles created: 20 Active, 10 Inactive, 20 Maintenance';
PRINT 'Trip history generated for all vehicles with completed status';
