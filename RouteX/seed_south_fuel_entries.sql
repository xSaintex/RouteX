-- Seed South Branch Fuel Entries (100+ entries from Jan 2025 to Mar 2026)

-- Get South Branch vehicles
DECLARE @Vehicles TABLE (VehicleId INT, PlateNumber NVARCHAR(50), UnitModel NVARCHAR(100), CurrentMileage DECIMAL(10,2));
INSERT INTO @Vehicles 
SELECT VehicleId, PlateNumber, UnitModel, Mileage 
FROM Vehicles 
WHERE BranchId = 3 
ORDER BY VehicleId;

-- Insert fuel entries with realistic data
INSERT INTO FuelEntries (VehicleId, Driver, DateTime, FuelStation, Odometer, Liters, TotalCost, FuelType, FullTank, Notes, UnitModel, PlateNumber, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT 
    v.VehicleId,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'Juan Santos'
        WHEN v.VehicleId % 6 = 1 THEN 'Maria Reyes'
        WHEN v.VehicleId % 6 = 2 THEN 'Carlos Cruz'
        WHEN v.VehicleId % 6 = 3 THEN 'Ana Martinez'
        WHEN v.VehicleId % 6 = 4 THEN 'Roberto Garcia'
        ELSE 'Elena Rodriguez'
    END as Driver,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 430), '2025-01-01') + 
    DATEADD(HOUR, ABS(CHECKSUM(NEWID()) % 14), '00:00:00') +
    DATEADD(MINUTE, ABS(CHECKSUM(NEWID()) % 60), '00:00:00') as DateTime,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'Caltex'
        WHEN v.VehicleId % 6 = 1 THEN 'Shell'
        WHEN v.VehicleId % 6 = 2 THEN 'Petron'
        WHEN v.VehicleId % 6 = 3 THEN 'Seaoil'
        WHEN v.VehicleId % 6 = 4 THEN 'Phoenix'
        ELSE 'Cleanoil'
    END as FuelStation,
    v.CurrentMileage - (ABS(CHECKSUM(NEWID()) % 5000) + 1000) as Odometer,
    ROUND(ABS(CHECKSUM(NEWID()) % 50) + 20, 2) as Liters,
    ROUND((ABS(CHECKSUM(NEWID()) % 50) + 20) * 
          CASE 
              WHEN v.VehicleId % 4 = 0 THEN 65.50  -- Diesel
              WHEN v.VehicleId % 4 = 1 THEN 72.80  -- Premium
              WHEN v.VehicleId % 4 = 2 THEN 68.90  -- Regular
              ELSE 71.20  -- Unleaded
          END, 2) as TotalCost,
    CASE 
        WHEN v.VehicleId % 4 = 0 THEN 'Diesel'
        WHEN v.VehicleId % 4 = 1 THEN 'Premium'
        WHEN v.VehicleId % 4 = 2 THEN 'Regular'
        ELSE 'Unleaded'
    END as FuelType,
    CASE 
        WHEN ABS(CHECKSUM(NEWID()) % 3) = 0 THEN 1
        ELSE 0
    END as FullTank,
    CASE 
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 0 THEN 'Regular maintenance refuel'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 1 THEN 'Long trip preparation'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 2 THEN 'Emergency fuel stop'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 3 THEN 'Weekly fueling schedule'
        ELSE 'Company vehicle refueling'
    END as Notes,
    v.UnitModel,
    v.PlateNumber,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 430), '2025-01-01') as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
CROSS JOIN (SELECT TOP 3 * FROM (SELECT 1 as n UNION SELECT 2 UNION SELECT 3) as x) as t
WHERE v.VehicleId <= 50  -- Ensure we only use the 50 vehicles
ORDER BY v.VehicleId, NEWID();

-- Insert additional entries for some vehicles to reach 100+
INSERT INTO FuelEntries (VehicleId, Driver, DateTime, FuelStation, Odometer, Liters, TotalCost, FuelType, FullTank, Notes, UnitModel, PlateNumber, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT TOP 50
    v.VehicleId,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'Juan Santos'
        WHEN v.VehicleId % 6 = 1 THEN 'Maria Reyes'
        WHEN v.VehicleId % 6 = 2 THEN 'Carlos Cruz'
        WHEN v.VehicleId % 6 = 3 THEN 'Ana Martinez'
        WHEN v.VehicleId % 6 = 4 THEN 'Roberto Garcia'
        ELSE 'Elena Rodriguez'
    END as Driver,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 430), '2025-01-01') + 
    DATEADD(HOUR, ABS(CHECKSUM(NEWID()) % 14), '00:00:00') +
    DATEADD(MINUTE, ABS(CHECKSUM(NEWID()) % 60), '00:00:00') as DateTime,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'Caltex'
        WHEN v.VehicleId % 6 = 1 THEN 'Shell'
        WHEN v.VehicleId % 6 = 2 THEN 'Petron'
        WHEN v.VehicleId % 6 = 3 THEN 'Seaoil'
        WHEN v.VehicleId % 6 = 4 THEN 'Phoenix'
        ELSE 'Cleanoil'
    END as FuelStation,
    v.CurrentMileage - (ABS(CHECKSUM(NEWID()) % 3000) + 500) as Odometer,
    ROUND(ABS(CHECKSUM(NEWID()) % 40) + 15, 2) as Liters,
    ROUND((ABS(CHECKSUM(NEWID()) % 40) + 15) * 
          CASE 
              WHEN v.VehicleId % 4 = 0 THEN 65.50  -- Diesel
              WHEN v.VehicleId % 4 = 1 THEN 72.80  -- Premium
              WHEN v.VehicleId % 4 = 2 THEN 68.90  -- Regular
              ELSE 71.20  -- Unleaded
          END, 2) as TotalCost,
    CASE 
        WHEN v.VehicleId % 4 = 0 THEN 'Diesel'
        WHEN v.VehicleId % 4 = 1 THEN 'Premium'
        WHEN v.VehicleId % 4 = 2 THEN 'Regular'
        ELSE 'Unleaded'
    END as FuelType,
    CASE 
        WHEN ABS(CHECKSUM(NEWID()) % 3) = 0 THEN 1
        ELSE 0
    END as FullTank,
    CASE 
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 0 THEN 'Monthly fuel report'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 1 THEN 'Special delivery fueling'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 2 THEN 'Weekend trip fuel'
        WHEN ABS(CHECKSUM(NEWID()) % 5) = 3 THEN 'Emergency backup fuel'
        ELSE 'Standard company refuel'
    END as Notes,
    v.UnitModel,
    v.PlateNumber,
    DATEADD(DAY, -ABS(CHECKSUM(NEWID()) % 430), '2025-01-01') as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
WHERE v.VehicleId <= 25  -- Additional entries for first 25 vehicles
ORDER BY v.VehicleId, NEWID();

PRINT 'South Branch fuel entries seeding completed!';
PRINT 'Fuel stations used: Caltex, Shell, Petron, Seaoil, Phoenix, Cleanoil';
PRINT 'Fuel types used: Diesel, Premium, Regular, Unleaded';
PRINT 'Date range: January 2025 to March 2026';
