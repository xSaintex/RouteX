-- Seed South Branch Maintenance Entries (100 entries with specific status distribution)

-- Get South Branch maintenance and inactive vehicles
DECLARE @Vehicles TABLE (VehicleId INT, PlateNumber NVARCHAR(50), UnitModel NVARCHAR(100), Status INT);
INSERT INTO @Vehicles 
SELECT VehicleId, PlateNumber, UnitModel, Status 
FROM Vehicles 
WHERE BranchId = 3 AND Status IN (1, 2) 
ORDER BY Status, VehicleId;

-- Insert 30 PENDING maintenance entries
INSERT INTO MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, MaintenanceId, UnitModel, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT 
    v.VehicleId,
    v.PlateNumber,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil Change'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake Inspection'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire Rotation'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery Replacement'
        WHEN v.VehicleId % 8 = 4 THEN 'Air Filter Replacement'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission Service'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant Flush'
        ELSE 'General Inspection'
    END as ServiceType,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-15'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-20'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-10'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-25'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-18'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-12'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-30'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-15'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-20'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-10'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-14'
        ELSE '2026-03-05'
    END as ServiceDate,
    ROUND(ABS(CHECKSUM(NEWID()) % 3000) + 500, 2) as Cost,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'AutoTech Pro'
        WHEN v.VehicleId % 6 = 1 THEN 'QuickFix Garage'
        WHEN v.VehicleId % 6 = 2 THEN 'MasterMechanic'
        WHEN v.VehicleId % 6 = 3 THEN 'ServiceCenter Plus'
        WHEN v.VehicleId % 6 = 4 THEN 'Expert Auto Care'
        ELSE 'ProService Garage'
    END as TechnicianName,
    0 as Status, -- PENDING
    ROUND(ABS(CHECKSUM(NEWID()) % 20000) + 5000, 0) as OdometerAtService,
    DATEADD(MONTH, 3 + ABS(CHECKSUM(NEWID()) % 3), 
        CASE 
            WHEN v.VehicleId % 12 = 0 THEN '2025-04-15'
            WHEN v.VehicleId % 12 = 1 THEN '2025-05-20'
            WHEN v.VehicleId % 12 = 2 THEN '2025-06-10'
            WHEN v.VehicleId % 12 = 3 THEN '2025-07-25'
            WHEN v.VehicleId % 12 = 4 THEN '2025-08-18'
            WHEN v.VehicleId % 12 = 5 THEN '2025-09-12'
            WHEN v.VehicleId % 12 = 6 THEN '2025-10-30'
            WHEN v.VehicleId % 12 = 7 THEN '2025-11-15'
            WHEN v.VehicleId % 12 = 8 THEN '2025-12-20'
            WHEN v.VehicleId % 12 = 9 THEN '2026-01-10'
            WHEN v.VehicleId % 12 = 10 THEN '2026-02-14'
            ELSE '2026-03-05'
        END) as NextServiceDue,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Routine oil change service with high-quality synthetic oil and filter replacement'
        WHEN v.VehicleId % 8 = 1 THEN 'Complete brake system inspection including pads, rotors, and fluid level check'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire rotation service to ensure even wear and extend tire life'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery replacement with new premium battery and terminal cleaning'
        WHEN v.VehicleId % 8 = 4 THEN 'Air filter replacement to improve engine performance and fuel efficiency'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission service including fluid change and filter replacement'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant system flush and refill with new coolant mixture'
        ELSE 'Comprehensive vehicle inspection covering all major systems and components'
    END as Description,
    ABS(CHECKSUM(NEWID()) % 10000) + 1000 as MaintenanceId,
    v.UnitModel,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-15'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-20'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-10'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-25'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-18'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-12'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-30'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-15'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-20'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-10'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-14'
        ELSE '2026-03-05'
    END as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
WHERE v.VehicleId <= 30  -- Use first 30 vehicles for pending entries
ORDER BY v.VehicleId;

-- Insert 20 ONGOING maintenance entries
INSERT INTO MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, MaintenanceId, UnitModel, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT 
    v.VehicleId,
    v.PlateNumber,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil Change'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake Inspection'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire Rotation'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery Replacement'
        WHEN v.VehicleId % 8 = 4 THEN 'Air Filter Replacement'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission Service'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant Flush'
        ELSE 'General Inspection'
    END as ServiceType,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-20'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-25'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-15'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-30'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-23'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-17'
        WHEN v.VehicleId % 12 = 6 THEN '2025-11-05'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-20'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-25'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-15'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-19'
        ELSE '2026-03-10'
    END as ServiceDate,
    ROUND(ABS(CHECKSUM(NEWID()) % 4000) + 800, 2) as Cost,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'AutoTech Pro'
        WHEN v.VehicleId % 6 = 1 THEN 'QuickFix Garage'
        WHEN v.VehicleId % 6 = 2 THEN 'MasterMechanic'
        WHEN v.VehicleId % 6 = 3 THEN 'ServiceCenter Plus'
        WHEN v.VehicleId % 6 = 4 THEN 'Expert Auto Care'
        ELSE 'ProService Garage'
    END as TechnicianName,
    1 as Status, -- ONGOING
    ROUND(ABS(CHECKSUM(NEWID()) % 25000) + 8000, 0) as OdometerAtService,
    DATEADD(MONTH, 4 + ABS(CHECKSUM(NEWID()) % 2), 
        CASE 
            WHEN v.VehicleId % 12 = 0 THEN '2025-04-20'
            WHEN v.VehicleId % 12 = 1 THEN '2025-05-25'
            WHEN v.VehicleId % 12 = 2 THEN '2025-06-15'
            WHEN v.VehicleId % 12 = 3 THEN '2025-07-30'
            WHEN v.VehicleId % 12 = 4 THEN '2025-08-23'
            WHEN v.VehicleId % 12 = 5 THEN '2025-09-17'
            WHEN v.VehicleId % 12 = 6 THEN '2025-11-05'
            WHEN v.VehicleId % 12 = 7 THEN '2025-11-20'
            WHEN v.VehicleId % 12 = 8 THEN '2025-12-25'
            WHEN v.VehicleId % 12 = 9 THEN '2026-01-15'
            WHEN v.VehicleId % 12 = 10 THEN '2026-02-19'
            ELSE '2026-03-10'
        END) as NextServiceDue,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil change in progress - vehicle currently on lift for service'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake inspection ongoing - technician checking brake system components'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire rotation service in progress - currently balancing tires'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery replacement procedure started - old battery removed'
        WHEN v.VehicleId % 8 = 4 THEN 'Air filter replacement in progress - technician accessing filter housing'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission service underway - fluid draining process started'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant flush procedure in progress - old coolant being drained'
        ELSE 'General inspection being performed - technician checking all vehicle systems'
    END as Description,
    ABS(CHECKSUM(NEWID()) % 10000) + 2000 as MaintenanceId,
    v.UnitModel,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-20'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-25'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-15'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-30'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-23'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-17'
        WHEN v.VehicleId % 12 = 6 THEN '2025-11-05'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-20'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-25'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-15'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-19'
        ELSE '2026-03-10'
    END as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
WHERE v.VehicleId BETWEEN 15 AND 34  -- Use next 20 vehicles for ongoing entries
ORDER BY v.VehicleId;

-- Insert 40 COMPLETED maintenance entries
INSERT INTO MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, MaintenanceId, UnitModel, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT 
    v.VehicleId,
    v.PlateNumber,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil Change'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake Inspection'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire Rotation'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery Replacement'
        WHEN v.VehicleId % 8 = 4 THEN 'Air Filter Replacement'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission Service'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant Flush'
        ELSE 'General Inspection'
    END as ServiceType,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-10'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-15'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-05'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-20'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-13'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-07'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-25'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-10'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-15'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-05'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-09'
        ELSE '2026-03-01'
    END as ServiceDate,
    ROUND(ABS(CHECKSUM(NEWID()) % 3500) + 600, 2) as Cost,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'AutoTech Pro'
        WHEN v.VehicleId % 6 = 1 THEN 'QuickFix Garage'
        WHEN v.VehicleId % 6 = 2 THEN 'MasterMechanic'
        WHEN v.VehicleId % 6 = 3 THEN 'ServiceCenter Plus'
        WHEN v.VehicleId % 6 = 4 THEN 'Expert Auto Care'
        ELSE 'ProService Garage'
    END as TechnicianName,
    2 as Status, -- COMPLETED
    ROUND(ABS(CHECKSUM(NEWID()) % 22000) + 6000, 0) as OdometerAtService,
    DATEADD(MONTH, 2 + ABS(CHECKSUM(NEWID()) % 4), 
        CASE 
            WHEN v.VehicleId % 12 = 0 THEN '2025-04-10'
            WHEN v.VehicleId % 12 = 1 THEN '2025-05-15'
            WHEN v.VehicleId % 12 = 2 THEN '2025-06-05'
            WHEN v.VehicleId % 12 = 3 THEN '2025-07-20'
            WHEN v.VehicleId % 12 = 4 THEN '2025-08-13'
            WHEN v.VehicleId % 12 = 5 THEN '2025-09-07'
            WHEN v.VehicleId % 12 = 6 THEN '2025-10-25'
            WHEN v.VehicleId % 12 = 7 THEN '2025-11-10'
            WHEN v.VehicleId % 12 = 8 THEN '2025-12-15'
            WHEN v.VehicleId % 12 = 9 THEN '2026-01-05'
            WHEN v.VehicleId % 12 = 10 THEN '2026-02-09'
            ELSE '2026-03-01'
        END) as NextServiceDue,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil change completed successfully with synthetic oil and new filter'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake inspection completed - all components checked and serviced as needed'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire rotation completed - all tires rotated and balanced properly'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery replacement completed - new battery installed and tested'
        WHEN v.VehicleId % 8 = 4 THEN 'Air filter replacement completed - engine breathing improved'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission service completed - fluid changed and system checked'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant flush completed - system refilled with proper coolant mixture'
        ELSE 'General inspection completed - all systems checked and vehicle deemed roadworthy'
    END as Description,
    ABS(CHECKSUM(NEWID()) % 10000) + 3000 as MaintenanceId,
    v.UnitModel,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-10'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-15'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-05'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-20'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-13'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-07'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-25'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-10'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-15'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-05'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-09'
        ELSE '2026-03-01'
    END as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
CROSS JOIN (SELECT TOP 2 * FROM (SELECT 1 as n UNION SELECT 2) as x) as t
WHERE v.VehicleId <= 20  -- Use first 20 vehicles for completed entries (2 each)
ORDER BY v.VehicleId, NEWID();

-- Insert 10 OVERDUE maintenance entries
INSERT INTO MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, MaintenanceId, UnitModel, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
SELECT 
    v.VehicleId,
    v.PlateNumber,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Oil Change'
        WHEN v.VehicleId % 8 = 1 THEN 'Brake Inspection'
        WHEN v.VehicleId % 8 = 2 THEN 'Tire Rotation'
        WHEN v.VehicleId % 8 = 3 THEN 'Battery Replacement'
        WHEN v.VehicleId % 8 = 4 THEN 'Air Filter Replacement'
        WHEN v.VehicleId % 8 = 5 THEN 'Transmission Service'
        WHEN v.VehicleId % 8 = 6 THEN 'Coolant Flush'
        ELSE 'General Inspection'
    END as ServiceType,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-05'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-10'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-01'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-15'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-08'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-02'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-20'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-05'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-10'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-01'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-05'
        ELSE '2026-02-25'
    END as ServiceDate,
    ROUND(ABS(CHECKSUM(NEWID()) % 2500) + 400, 2) as Cost,
    CASE 
        WHEN v.VehicleId % 6 = 0 THEN 'AutoTech Pro'
        WHEN v.VehicleId % 6 = 1 THEN 'QuickFix Garage'
        WHEN v.VehicleId % 6 = 2 THEN 'MasterMechanic'
        WHEN v.VehicleId % 6 = 3 THEN 'ServiceCenter Plus'
        WHEN v.VehicleId % 6 = 4 THEN 'Expert Auto Care'
        ELSE 'ProService Garage'
    END as TechnicianName,
    3 as Status, -- OVERDUE
    ROUND(ABS(CHECKSUM(NEWID()) % 18000) + 4000, 0) as OdometerAtService,
    DATEADD(MONTH, -1 + ABS(CHECKSUM(NEWID()) % 2), 
        CASE 
            WHEN v.VehicleId % 12 = 0 THEN '2025-04-05'
            WHEN v.VehicleId % 12 = 1 THEN '2025-05-10'
            WHEN v.VehicleId % 12 = 2 THEN '2025-06-01'
            WHEN v.VehicleId % 12 = 3 THEN '2025-07-15'
            WHEN v.VehicleId % 12 = 4 THEN '2025-08-08'
            WHEN v.VehicleId % 12 = 5 THEN '2025-09-02'
            WHEN v.VehicleId % 12 = 6 THEN '2025-10-20'
            WHEN v.VehicleId % 12 = 7 THEN '2025-11-05'
            WHEN v.VehicleId % 12 = 8 THEN '2025-12-10'
            WHEN v.VehicleId % 12 = 9 THEN '2026-01-01'
            WHEN v.VehicleId % 12 = 10 THEN '2026-02-05'
            ELSE '2026-02-25'
        END) as NextServiceDue,
    CASE 
        WHEN v.VehicleId % 8 = 0 THEN 'Overdue oil change - vehicle needs immediate attention to prevent engine damage'
        WHEN v.VehicleId % 8 = 1 THEN 'Overdue brake inspection - safety concern, requires immediate service'
        WHEN v.VehicleId % 8 = 2 THEN 'Overdue tire rotation - uneven tire wear detected, service needed soon'
        WHEN v.VehicleId % 8 = 3 THEN 'Overdue battery replacement - battery showing signs of failure'
        WHEN v.VehicleId % 8 = 4 THEN 'Overdue air filter replacement - engine performance affected'
        WHEN v.VehicleId % 8 = 5 THEN 'Overdue transmission service - potential transmission damage risk'
        WHEN v.VehicleId % 8 = 6 THEN 'Overdue coolant flush - overheating risk, service critical'
        ELSE 'Overdue general inspection - multiple service items need attention'
    END as Description,
    ABS(CHECKSUM(NEWID()) % 10000) + 4000 as MaintenanceId,
    v.UnitModel,
    CASE 
        WHEN v.VehicleId % 12 = 0 THEN '2025-04-05'
        WHEN v.VehicleId % 12 = 1 THEN '2025-05-10'
        WHEN v.VehicleId % 12 = 2 THEN '2025-06-01'
        WHEN v.VehicleId % 12 = 3 THEN '2025-07-15'
        WHEN v.VehicleId % 12 = 4 THEN '2025-08-08'
        WHEN v.VehicleId % 12 = 5 THEN '2025-09-02'
        WHEN v.VehicleId % 12 = 6 THEN '2025-10-20'
        WHEN v.VehicleId % 12 = 7 THEN '2025-11-05'
        WHEN v.VehicleId % 12 = 8 THEN '2025-12-10'
        WHEN v.VehicleId % 12 = 9 THEN '2026-01-01'
        WHEN v.VehicleId % 12 = 10 THEN '2026-02-05'
        ELSE '2026-02-25'
    END as Date,
    GETDATE() as CreatedDate,
    GETDATE() as ModifiedDate,
    0 as IsArchived,
    3 as BranchId
FROM @Vehicles v
WHERE v.VehicleId BETWEEN 25 AND 34  -- Use last 10 vehicles for overdue entries
ORDER BY v.VehicleId;

PRINT 'South Branch maintenance entries seeding completed!';
PRINT 'Status distribution: 30 Pending, 20 Ongoing, 40 Completed, 10 Overdue';
PRINT 'Service types: Oil Change, Brake Inspection, Tire Rotation, Battery Replacement, Air Filter Replacement, Transmission Service, Coolant Flush, General Inspection';
PRINT 'Date range: April 2025 to March 2026';
PRINT 'Next service due dates are set to be past the service date as requested';
