-- Seed South Branch (BranchId = 3) with 50 vehicles and trip history

-- Insert 20 Active Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, VehicleType, Status, Mileage, IsArchived, BranchId, CreatedDate, ModifiedDate, AddedByUserId) VALUES
('SBR-1001', 'Toyota Vios', 'Sedan', 0, 15420.50, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1002', 'Honda Civic', 'Sedan', 0, 23150.75, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1003', 'Mitsubishi Mirage', 'Sedan', 0, 8920.25, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1004', 'Nissan Sentra', 'Sedan', 0, 31200.00, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1005', 'Ford Ranger', 'Truck', 0, 45600.80, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1006', 'Isuzu D-Max', 'Truck', 0, 28900.60, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1007', 'Toyota Hilux', 'Truck', 0, 51200.90, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1008', 'Honda CR-V', 'SUV', 0, 19800.40, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1009', 'Mitsubishi Xpander', 'SUV', 0, 12300.20, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1010', 'Nissan Navara', 'Truck', 0, 38700.55, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1011', 'Ford Everest', 'SUV', 0, 42100.30, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1012', 'Isuzu Mu-X', 'SUV', 0, 35600.70, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1013', 'Toyota Innova', 'SUV', 0, 27800.85, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1014', 'Honda City', 'Sedan', 0, 16500.45, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1015', 'Mitsubishi L300', 'Van', 0, 52300.95, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1016', 'Nissan Urvan', 'Van', 0, 48900.65, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1017', 'Ford Transit', 'Van', 0, 41200.25, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1018', 'Isuzu Crosswind', 'SUV', 0, 36700.80, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1019', 'Toyota Avanza', 'SUV', 0, 21300.35, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-1020', 'Honda Jazz', 'Sedan', 0, 14200.15, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1);

-- Insert 10 Inactive Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, VehicleType, Status, Mileage, IsArchived, BranchId, CreatedDate, ModifiedDate, AddedByUserId) VALUES
('SBR-2001', 'Toyota Corolla', 'Sedan', 1, 89000.00, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2002', 'Honda Accord', 'Sedan', 1, 76500.50, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2003', 'Mitsubishi Lancer', 'Sedan', 1, 92300.75, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2004', 'Nissan Altima', 'Sedan', 1, 81400.25, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2005', 'Ford Focus', 'Sedan', 1, 67800.80, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2006', 'Isuzu Gemini', 'Sedan', 1, 95200.40, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2007', 'Toyota Camry', 'Sedan', 1, 88700.60, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2008', 'Honda Prelude', 'Sedan', 1, 94600.90, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2009', 'Mitsubishi Galant', 'Sedan', 1, 72300.30, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-2010', 'Nissan Maxima', 'Sedan', 1, 89500.55, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1);

-- Insert 20 Maintenance Vehicles
INSERT INTO Vehicles (PlateNumber, UnitModel, VehicleType, Status, Mileage, IsArchived, BranchId, CreatedDate, ModifiedDate, AddedByUserId) VALUES
('SBR-3001', 'Toyota Hiace', 'Van', 2, 67800.45, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3002', 'Honda Mobilio', 'SUV', 2, 54300.70, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3003', 'Mitsubishi Adventure', 'SUV', 2, 78900.85, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3004', 'Nissan Patrol', 'SUV', 2, 91200.20, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3005', 'Ford Expedition', 'SUV', 2, 83400.95, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3006', 'Isuzu Trooper', 'SUV', 2, 75600.15, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3007', 'Toyota Revo', 'SUV', 2, 62300.40, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3008', 'Honda HR-V', 'SUV', 2, 57800.60, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3009', 'Mitsubishi Montero', 'SUV', 2, 84500.80, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3010', 'Nissan X-Trail', 'SUV', 2, 71200.25, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3011', 'Ford Escape', 'SUV', 2, 68900.35, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3012', 'Isuzu Fuego', 'Truck', 2, 73400.50, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3013', 'Toyota Fortuner', 'SUV', 2, 86700.75, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3014', 'Honda BR-V', 'SUV', 2, 59200.90, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3015', 'Mitsubishi Strada', 'Truck', 2, 82300.10, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3016', 'Nissan Terra', 'SUV', 2, 79800.30, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3017', 'Ford Ranger Raptor', 'Truck', 2, 93400.65, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3018', 'Isuzu D-Max', 'Truck', 2, 87600.85, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3019', 'Toyota Land Cruiser', 'SUV', 2, 94500.20, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1),
('SBR-3020', 'Honda Pilot', 'SUV', 2, 88900.40, 0, 3, '2026-03-09 22:00:00', '2026-03-09 22:00:00', 1);

-- Get vehicle IDs for trip history generation
DECLARE @VehicleIds TABLE (VehicleId INT);
INSERT INTO @VehicleIds SELECT VehicleId FROM Vehicles WHERE PlateNumber LIKE 'SBR-%' ORDER BY VehicleId;

-- Generate trip history for all vehicles (3-5 trips per vehicle)
-- Active Vehicles (SBR-1001 to SBR-1020) - 3 trips each
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 20
    VehicleId,
    'Makati City Hall, Makati',
    'Bonifacio Global City, Taguig',
    ROUND(15 + RAND() * 25, 2),
    2,
    3,
    '2026-02-01 10:00:00',
    '2026-02-01 10:00:00',
    '2026-02-01 12:00:00'
FROM @VehicleIds ORDER BY VehicleId;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 20
    VehicleId,
    'Ayala Avenue, Makati',
    'Ortigas Center, Pasig',
    ROUND(20 + RAND() * 30, 2),
    2,
    3,
    '2026-01-15 09:00:00',
    '2026-01-15 09:00:00',
    '2026-01-15 11:30:00'
FROM @VehicleIds ORDER BY VehicleId;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 20
    VehicleId,
    'Makati Medical Center, Makati',
    'St. Luke Medical Center, Quezon City',
    ROUND(25 + RAND() * 35, 2),
    2,
    3,
    '2026-01-01 08:00:00',
    '2026-01-01 08:00:00',
    '2026-01-01 10:45:00'
FROM @VehicleIds ORDER BY VehicleId;

-- Inactive Vehicles (SBR-2001 to SBR-2010) - 2 trips each
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 10
    VehicleId,
    'Makati City Hall, Makati',
    'Manila City Hall, Manila',
    ROUND(18 + RAND() * 22, 2),
    2,
    3,
    '2025-12-01 09:00:00',
    '2025-12-01 09:00:00',
    '2025-12-01 11:15:00'
FROM @VehicleIds WHERE VehicleId > (SELECT MAX(VehicleId) FROM (SELECT TOP 20 VehicleId FROM @VehicleIds ORDER BY VehicleId) AS Top20) ORDER BY VehicleId;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 10
    VehicleId,
    'Greenbelt Mall, Makati',
    'SM Megamall, Mandaluyong',
    ROUND(22 + RAND() * 28, 2),
    2,
    3,
    '2025-11-15 14:00:00',
    '2025-11-15 14:00:00',
    '2025-11-15 16:30:00'
FROM @VehicleIds WHERE VehicleId > (SELECT MAX(VehicleId) FROM (SELECT TOP 20 VehicleId FROM @VehicleIds ORDER BY VehicleId) AS Top20) ORDER BY VehicleId;

-- Maintenance Vehicles (SBR-3001 to SBR-3020) - 2 trips each
INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 20
    VehicleId,
    'Makati Central Business District',
    'Pasay City',
    ROUND(12 + RAND() * 18, 2),
    2,
    3,
    '2026-02-15 11:00:00',
    '2026-02-15 11:00:00',
    '2026-02-15 13:00:00'
FROM @VehicleIds WHERE VehicleId > (SELECT MAX(VehicleId) FROM (SELECT TOP 30 VehicleId FROM @VehicleIds ORDER BY VehicleId) AS Top30) ORDER BY VehicleId;

INSERT INTO RouteTrips (VehicleId, StartAddress, EndAddress, DistanceKm, Status, BranchId, CreatedDate, ModifiedDate, CompletedAt)
SELECT TOP 20
    VehicleId,
    'Makati Avenue, Makati',
    'EDSA, Mandaluyong',
    ROUND(28 + RAND() * 32, 2),
    2,
    3,
    '2026-01-20 08:30:00',
    '2026-01-20 08:30:00',
    '2026-01-20 11:45:00'
FROM @VehicleIds WHERE VehicleId > (SELECT MAX(VehicleId) FROM (SELECT TOP 30 VehicleId FROM @VehicleIds ORDER BY VehicleId) AS Top30) ORDER BY VehicleId;

PRINT 'South Branch data seeding completed successfully!';
PRINT '50 vehicles created: 20 Active, 10 Inactive, 20 Maintenance';
PRINT 'Trip history generated for all vehicles with completed status';
