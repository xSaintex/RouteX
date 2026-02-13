-- =============================================
-- RouteX Database Schema for SQL Server 2025
-- Generated: February 13, 2026
-- FIXED VERSION - All SQL Server batch issues resolved
-- =============================================

-- Drop existing tables if they exist (for clean recreation)
IF OBJECT_ID('dbo.FinanceEntries', 'U') IS NOT NULL DROP TABLE dbo.FinanceEntries;
IF OBJECT_ID('dbo.MaintenanceEntries', 'U') IS NOT NULL DROP TABLE dbo.MaintenanceEntries;
IF OBJECT_ID('dbo.FuelEntries', 'U') IS NOT NULL DROP TABLE dbo.FuelEntries;
IF OBJECT_ID('dbo.AuditLogs', 'U') IS NOT NULL DROP TABLE dbo.AuditLogs;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Vehicles', 'U') IS NOT NULL DROP TABLE dbo.Vehicles;
GO

-- =============================================
-- 1. VEHICLES TABLE
-- =============================================
CREATE TABLE dbo.Vehicles
(
    VehicleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UnitModel NVARCHAR(100) NOT NULL,
    PlateNumber NVARCHAR(50) NOT NULL,
    VehicleType NVARCHAR(50) NOT NULL,
    Status INT NOT NULL, -- 0=Active, 1=Maintenance, 2=Inactive (VehicleStatus enum)
    Mileage INT NOT NULL DEFAULT 0,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- Add indexes for Vehicles
CREATE INDEX IX_Vehicles_PlateNumber ON dbo.Vehicles(PlateNumber);
CREATE INDEX IX_Vehicles_Status ON dbo.Vehicles(Status);
GO

-- =============================================
-- 2. USERS TABLE
-- =============================================
CREATE TABLE dbo.Users
(
    UserId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Active',
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE()
);
GO

-- Add indexes for Users
CREATE INDEX IX_Users_Email ON dbo.Users(Email);
CREATE INDEX IX_Users_Status ON dbo.Users(Status);
GO

-- =============================================
-- 3. AUDIT LOGS TABLE
-- =============================================
CREATE TABLE dbo.AuditLogs
(
    AuditLogId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL, -- Can reference AspNetUsers or custom Users
    Action NVARCHAR(500) NOT NULL,
    ActionDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    TableName NVARCHAR(100) NULL,
    RecordId INT NULL
);
GO

-- Add indexes for AuditLogs
CREATE INDEX IX_AuditLogs_UserId ON dbo.AuditLogs(UserId);
CREATE INDEX IX_AuditLogs_ActionDate ON dbo.AuditLogs(ActionDate);
CREATE INDEX IX_AuditLogs_TableName ON dbo.AuditLogs(TableName);
GO

-- =============================================
-- 4. FUEL ENTRIES TABLE
-- =============================================
CREATE TABLE dbo.FuelEntries
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VehicleId INT NOT NULL,
    Driver NVARCHAR(100) NOT NULL,
    DateTime DATETIME2 NOT NULL,
    FuelStation NVARCHAR(200) NOT NULL,
    Odometer INT NOT NULL,
    Liters DECIMAL(10,2) NOT NULL,
    TotalCost DECIMAL(18,2) NOT NULL,
    FuelType NVARCHAR(50) NOT NULL DEFAULT 'Regular',
    FullTank BIT NOT NULL DEFAULT 1,
    Notes NVARCHAR(1000) NULL,
    -- Legacy properties for backward compatibility
    UnitModel NVARCHAR(100) NULL,
    PlateNumber NVARCHAR(50) NULL,
    Date DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Key constraint
    CONSTRAINT FK_FuelEntries_Vehicles FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles(VehicleId) ON DELETE CASCADE
);
GO

-- Add indexes for FuelEntries
CREATE INDEX IX_FuelEntries_VehicleId ON dbo.FuelEntries(VehicleId);
CREATE INDEX IX_FuelEntries_DateTime ON dbo.FuelEntries(DateTime);
CREATE INDEX IX_FuelEntries_Odometer ON dbo.FuelEntries(Odometer);
GO

-- =============================================
-- 5. MAINTENANCE ENTRIES TABLE
-- =============================================
CREATE TABLE dbo.MaintenanceEntries
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VehicleId INT NOT NULL,
    PlateNumber NVARCHAR(50) NOT NULL,
    ServiceType NVARCHAR(200) NOT NULL,
    ServiceDate DATETIME2 NOT NULL,
    Cost DECIMAL(18,2) NOT NULL,
    TechnicianName NVARCHAR(200) NOT NULL,
    Status INT NOT NULL, -- 0=Pending, 1=Ongoing, 2=Completed, 3=Overdue (MaintenanceStatus enum)
    OdometerAtService INT NOT NULL,
    NextServiceDue DATETIME2 NULL,
    Description NVARCHAR(1000) NULL,
    -- Legacy properties for backward compatibility
    MaintenanceId INT NULL,
    UnitModel NVARCHAR(100) NULL,
    Date DATETIME2 NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Key constraint
    CONSTRAINT FK_MaintenanceEntries_Vehicles FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles(VehicleId) ON DELETE CASCADE
);
GO

-- Add indexes for MaintenanceEntries
CREATE INDEX IX_MaintenanceEntries_VehicleId ON dbo.MaintenanceEntries(VehicleId);
CREATE INDEX IX_MaintenanceEntries_ServiceDate ON dbo.MaintenanceEntries(ServiceDate);
CREATE INDEX IX_MaintenanceEntries_Status ON dbo.MaintenanceEntries(Status);
CREATE INDEX IX_MaintenanceEntries_NextServiceDue ON dbo.MaintenanceEntries(NextServiceDue);
GO

-- =============================================
-- 6. FINANCE ENTRIES TABLE
-- =============================================
CREATE TABLE dbo.FinanceEntries
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    VehicleId INT NOT NULL,
    ExpenseType NVARCHAR(50) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    ExpenseDate DATE NOT NULL,
    Description NVARCHAR(500) NULL,
    ReferenceId INT NULL, -- For Fuel ID or Maintenance ID reference
    AttachmentPath NVARCHAR(500) NULL,
    CreatedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    ModifiedDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    
    -- Foreign Key constraint
    CONSTRAINT FK_FinanceEntries_Vehicles FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles(VehicleId) ON DELETE CASCADE
);
GO

-- Add indexes for FinanceEntries
CREATE INDEX IX_FinanceEntries_VehicleId ON dbo.FinanceEntries(VehicleId);
CREATE INDEX IX_FinanceEntries_ExpenseDate ON dbo.FinanceEntries(ExpenseDate);
CREATE INDEX IX_FinanceEntries_ExpenseType ON dbo.FinanceEntries(ExpenseType);
CREATE INDEX IX_FinanceEntries_ReferenceId ON dbo.FinanceEntries(ReferenceId);
GO

-- =============================================
-- SAMPLE DATA INSERTIONS
-- =============================================

-- Insert sample vehicles
INSERT INTO dbo.Vehicles (UnitModel, PlateNumber, VehicleType, Status, Mileage) VALUES
('Isuzu N-Series', 'TRK-001', 'Truck', 0, 45000),
('Toyota Hilux', 'TRK-002', 'Pickup', 0, 32000),
('Mitsubishi L300', 'TRK-003', 'Van', 1, 67000),
('Ford Ranger', 'TRK-004', 'Pickup', 0, 28000),
('Nissan Urvan', 'TRK-005', 'Van', 2, 89000);
GO

-- Insert sample users
INSERT INTO dbo.Users (FirstName, LastName, Email, Status) VALUES
('Juan', 'Santos', 'juan.santos@routex.com', 'Active'),
('Maria', 'Reyes', 'maria.reyes@routex.com', 'Active'),
('Carlos', 'Garcia', 'carlos.garcia@routex.com', 'Active'),
('Ana', 'Lopez', 'ana.lopez@routex.com', 'Inactive');
GO

-- Insert sample fuel entries
INSERT INTO dbo.FuelEntries (VehicleId, Driver, DateTime, FuelStation, Odometer, Liters, TotalCost, FuelType, FullTank, Notes, UnitModel, PlateNumber, Date) VALUES
(1, 'Juan Santos', '2026-01-15 08:30:00', 'Shell Station', 45200, 45.50, 2450.75, 'Diesel', 1, 'Regular fueling', 'Isuzu N-Series', 'TRK-001', '2026-01-15'),
(2, 'Maria Reyes', '2026-01-16 09:15:00', 'Petron Station', 32200, 38.20, 2156.80, 'Gasoline', 1, 'City route fueling', 'Toyota Hilux', 'TRK-002', '2026-01-16'),
(1, 'Juan Santos', '2026-01-20 07:45:00', 'Caltex Station', 45800, 42.80, 2389.60, 'Diesel', 1, 'Highway fueling', 'Isuzu N-Series', 'TRK-001', '2026-01-20');
GO

-- Insert sample maintenance entries
INSERT INTO dbo.MaintenanceEntries (VehicleId, PlateNumber, ServiceType, ServiceDate, Cost, TechnicianName, Status, OdometerAtService, NextServiceDue, Description, MaintenanceId, UnitModel, Date) VALUES
(1, 'TRK-001', 'Oil Change', '2026-01-10', 1500.00, 'AutoTech Center', 2, 45000, '2026-04-10', 'Regular oil change with filter replacement', 1, 'Isuzu N-Series', '2026-01-10'),
(2, 'TRK-002', 'Tire Rotation', '2026-01-12', 800.00, 'Quick Service', 2, 32000, '2026-04-12', 'Tire rotation and balancing', 2, 'Toyota Hilux', '2026-01-12'),
(3, 'TRK-003', 'Brake Repair', '2026-01-15', 3500.00, 'Expert Garage', 1, 67000, '2026-04-15', 'Front brake pad replacement', 3, 'Mitsubishi L300', '2026-01-15');
GO

-- Insert sample finance entries
INSERT INTO dbo.FinanceEntries (VehicleId, ExpenseType, Amount, ExpenseDate, Description, ReferenceId) VALUES
(1, 'Fuel', 2450.75, '2026-01-15', 'Diesel fuel purchase', 1),
(2, 'Fuel', 2156.80, '2026-01-16', 'Gasoline fuel purchase', 2),
(1, 'Fuel', 2389.60, '2026-01-20', 'Diesel fuel purchase', 3),
(1, 'Maintenance', 1500.00, '2026-01-10', 'Oil change service', 1),
(2, 'Maintenance', 800.00, '2026-01-12', 'Tire rotation', 2),
(3, 'Maintenance', 3500.00, '2026-01-15', 'Brake repair', 3);
GO

-- Insert sample audit logs
INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, IPAddress, TableName, RecordId) VALUES
('1', 'CREATE', '2026-01-15 08:30:00', '192.168.1.100', 'FuelEntries', 1),
('2', 'CREATE', '2026-01-16 09:15:00', '192.168.1.101', 'FuelEntries', 2),
('1', 'CREATE', '2026-01-20 07:45:00', '192.168.1.100', 'FuelEntries', 3),
('1', 'CREATE', '2026-01-10 10:00:00', '192.168.1.100', 'MaintenanceEntries', 1),
('2', 'CREATE', '2026-01-12 14:30:00', '192.168.1.101', 'MaintenanceEntries', 2);
GO

-- =============================================
-- VIEWS FOR COMMON QUERIES
-- =============================================

GO

-- View: Vehicle Summary with latest status
CREATE VIEW dbo.VehicleSummary AS
SELECT 
    v.VehicleId,
    v.UnitModel,
    v.PlateNumber,
    v.VehicleType,
    CASE v.Status 
        WHEN 0 THEN 'Active'
        WHEN 1 THEN 'Maintenance' 
        WHEN 2 THEN 'Inactive'
        ELSE 'Unknown'
    END AS StatusText,
    v.Mileage,
    v.CreatedDate,
    -- Latest fuel entry info
    (SELECT TOP 1 DateTime FROM dbo.FuelEntries fe WHERE fe.VehicleId = v.VehicleId ORDER BY DateTime DESC) AS LastFuelDate,
    (SELECT TOP 1 TotalCost FROM dbo.FuelEntries fe WHERE fe.VehicleId = v.VehicleId ORDER BY DateTime DESC) AS LastFuelCost,
    -- Latest maintenance info
    (SELECT TOP 1 ServiceDate FROM dbo.MaintenanceEntries me WHERE me.VehicleId = v.VehicleId ORDER BY ServiceDate DESC) AS LastMaintenanceDate,
    (SELECT TOP 1 Cost FROM dbo.MaintenanceEntries me WHERE me.VehicleId = v.VehicleId ORDER BY ServiceDate DESC) AS LastMaintenanceCost
FROM dbo.Vehicles v;
GO

-- View: Fuel Expenses Summary
CREATE VIEW dbo.FuelExpensesSummary AS
SELECT 
    fe.Id,
    fe.VehicleId,
    v.UnitModel,
    v.PlateNumber,
    fe.Driver,
    fe.DateTime,
    fe.FuelStation,
    fe.Odometer,
    fe.Liters,
    fe.TotalCost,
    fe.FuelType,
    fe.FullTank,
    fe.Notes,
    -- Calculate cost per liter
    CASE WHEN fe.Liters > 0 THEN fe.TotalCost / fe.Liters ELSE 0 END AS CostPerLiter
FROM dbo.FuelEntries fe
INNER JOIN dbo.Vehicles v ON fe.VehicleId = v.VehicleId;
GO

-- View: Maintenance Summary
CREATE VIEW dbo.MaintenanceSummary AS
SELECT 
    me.Id,
    me.VehicleId,
    v.UnitModel,
    v.PlateNumber,
    me.ServiceType,
    me.ServiceDate,
    me.Cost,
    me.TechnicianName,
    CASE me.Status
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Ongoing'
        WHEN 2 THEN 'Completed'
        WHEN 3 THEN 'Overdue'
        ELSE 'Unknown'
    END AS StatusText,
    me.OdometerAtService,
    me.NextServiceDue,
    me.Description
FROM dbo.MaintenanceEntries me
INNER JOIN dbo.Vehicles v ON me.VehicleId = v.VehicleId;
GO

-- View: Finance Summary by Vehicle
CREATE VIEW dbo.FinanceSummaryByVehicle AS
SELECT 
    v.VehicleId,
    v.UnitModel,
    v.PlateNumber,
    -- Total fuel costs
    ISNULL((SELECT SUM(TotalCost) FROM dbo.FuelEntries WHERE VehicleId = v.VehicleId), 0) AS TotalFuelCost,
    -- Total maintenance costs
    ISNULL((SELECT SUM(Cost) FROM dbo.MaintenanceEntries WHERE VehicleId = v.VehicleId), 0) AS TotalMaintenanceCost,
    -- Total other expenses
    ISNULL((SELECT SUM(Amount) FROM dbo.FinanceEntries 
            WHERE VehicleId = v.VehicleId AND ExpenseType NOT IN ('Fuel', 'Maintenance')), 0) AS TotalOtherCost,
    -- Grand total
    ISNULL((SELECT SUM(TotalCost) FROM dbo.FuelEntries WHERE VehicleId = v.VehicleId), 0) +
    ISNULL((SELECT SUM(Cost) FROM dbo.MaintenanceEntries WHERE VehicleId = v.VehicleId), 0) +
    ISNULL((SELECT SUM(Amount) FROM dbo.FinanceEntries 
            WHERE VehicleId = v.VehicleId AND ExpenseType NOT IN ('Fuel', 'Maintenance')), 0) AS TotalCost,
    -- Counts
    (SELECT COUNT(*) FROM dbo.FuelEntries WHERE VehicleId = v.VehicleId) AS FuelEntryCount,
    (SELECT COUNT(*) FROM dbo.MaintenanceEntries WHERE VehicleId = v.VehicleId) AS MaintenanceEntryCount
FROM dbo.Vehicles v;
GO

-- =============================================
-- STORED PROCEDURES FOR COMMON OPERATIONS
-- =============================================

GO

-- Procedure: Get Vehicle Expenses by Date Range
CREATE PROCEDURE dbo.GetVehicleExpensesByDateRange
    @StartDate DATE,
    @EndDate DATE,
    @VehicleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VehicleId,
        v.UnitModel,
        v.PlateNumber,
        'Fuel' AS ExpenseType,
        fe.DateTime AS ExpenseDate,
        fe.TotalCost AS Amount,
        fe.FuelStation AS Vendor,
        fe.Notes AS Description
    FROM dbo.Vehicles v
    INNER JOIN dbo.FuelEntries fe ON v.VehicleId = fe.VehicleId
    WHERE CAST(fe.DateTime AS DATE) BETWEEN @StartDate AND @EndDate
    AND (@VehicleId IS NULL OR v.VehicleId = @VehicleId)
    
    UNION ALL
    
    SELECT 
        v.VehicleId,
        v.UnitModel,
        v.PlateNumber,
        'Maintenance' AS ExpenseType,
        me.ServiceDate AS ExpenseDate,
        me.Cost AS Amount,
        me.TechnicianName AS Vendor,
        me.Description
    FROM dbo.Vehicles v
    INNER JOIN dbo.MaintenanceEntries me ON v.VehicleId = me.VehicleId
    WHERE me.ServiceDate BETWEEN @StartDate AND @EndDate
    AND (@VehicleId IS NULL OR v.VehicleId = @VehicleId)
    
    UNION ALL
    
    SELECT 
        v.VehicleId,
        v.UnitModel,
        v.PlateNumber,
        fe.ExpenseType,
        fe.ExpenseDate,
        fe.Amount,
        NULL AS Vendor,
        fe.Description
    FROM dbo.Vehicles v
    INNER JOIN dbo.FinanceEntries fe ON v.VehicleId = fe.VehicleId
    WHERE fe.ExpenseDate BETWEEN @StartDate AND @EndDate
    AND (@VehicleId IS NULL OR v.VehicleId = @VehicleId)
    
    ORDER BY ExpenseDate DESC;
END;
GO

-- Procedure: Get Maintenance Schedule
CREATE PROCEDURE dbo.GetMaintenanceSchedule
    @DaysAhead INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        v.VehicleId,
        v.UnitModel,
        v.PlateNumber,
        me.ServiceType,
        me.ServiceDate,
        me.NextServiceDue,
        me.Cost,
        CASE me.Status
            WHEN 0 THEN 'Pending'
            WHEN 1 THEN 'Ongoing'
            WHEN 2 THEN 'Completed'
            WHEN 3 THEN 'Overdue'
            ELSE 'Unknown'
        END AS StatusText,
        CASE 
            WHEN me.NextServiceDue < GETDATE() THEN 'Overdue'
            WHEN me.NextServiceDue BETWEEN GETDATE() AND DATEADD(DAY, @DaysAhead, GETDATE()) THEN 'Due Soon'
            ELSE 'Scheduled'
        END AS Urgency,
        DATEDIFF(DAY, GETDATE(), me.NextServiceDue) AS DaysUntilDue
    FROM dbo.Vehicles v
    INNER JOIN dbo.MaintenanceEntries me ON v.VehicleId = me.VehicleId
    WHERE me.NextServiceDue IS NOT NULL
    AND me.Status != 2 -- Not completed
    ORDER BY me.NextServiceDue ASC;
END;
GO

-- Procedure: Get Fuel Efficiency Report
CREATE PROCEDURE dbo.GetFuelEfficiencyReport
    @VehicleId INT = NULL,
    @StartDate DATE = NULL,
    @EndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    WITH FuelData AS (
        SELECT 
            fe.VehicleId AS VehicleIdCol,
            v.UnitModel,
            v.PlateNumber,
            fe.DateTime,
            fe.Odometer,
            fe.Liters,
            fe.TotalCost,
            LAG(fe.Odometer) OVER (PARTITION BY fe.VehicleId ORDER BY fe.DateTime) AS PrevOdometer,
            LAG(fe.DateTime) OVER (PARTITION BY fe.VehicleId ORDER BY fe.DateTime) AS PrevDateTime
        FROM dbo.FuelEntries fe
        INNER JOIN dbo.Vehicles v ON fe.VehicleId = v.VehicleId
        WHERE (@VehicleId IS NULL OR fe.VehicleId = @VehicleId)
        AND (@StartDate IS NULL OR CAST(fe.DateTime AS DATE) >= @StartDate)
        AND (@EndDate IS NULL OR CAST(fe.DateTime AS DATE) <= @EndDate)
        AND fe.FullTank = 1 -- Only full tank calculations
    )
    SELECT 
        VehicleIdCol AS VehicleId,
        UnitModel,
        PlateNumber,
        DateTime,
        Odometer,
        Liters,
        TotalCost,
        Odometer - PrevOdometer AS DistanceTraveled,
        CASE WHEN PrevOdometer IS NOT NULL AND Liters > 0 
             THEN (Odometer - PrevOdometer) / Liters 
             ELSE NULL END AS KmPerLiter,
        CASE WHEN PrevOdometer IS NOT NULL AND Liters > 0 
             THEN (Odometer - PrevOdometer) / (Liters * 3.78541) 
             ELSE NULL END AS MilesPerGallon,
        CASE WHEN PrevOdometer IS NOT NULL AND (Odometer - PrevOdometer) > 0 
             THEN (Liters * 100) / (Odometer - PrevOdometer) 
             ELSE NULL END AS LitersPer100Km,
        CASE WHEN TotalCost > 0 AND (Odometer - PrevOdometer) > 0 
             THEN TotalCost / (Odometer - PrevOdometer) * 100 
             ELSE NULL END AS CostPer100Km
    FROM FuelData
    WHERE PrevOdometer IS NOT NULL
    AND (Odometer - PrevOdometer) > 0
    ORDER BY VehicleIdCol, DateTime DESC;
END;
GO

-- =============================================
-- TRIGGERS FOR AUDITING
-- =============================================

GO

-- Trigger: Audit Fuel Entries
CREATE TRIGGER dbo.TR_FuelEntries_Audit
ON dbo.FuelEntries
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        IF EXISTS (SELECT * FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'UPDATE', GETDATE(), 'FuelEntries', Id FROM inserted;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'CREATE', GETDATE(), 'FuelEntries', Id FROM inserted;
        END
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        -- DELETE
        INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
        SELECT 'SYSTEM', 'DELETE', GETDATE(), 'FuelEntries', Id FROM deleted;
    END
END;
GO

-- Trigger: Audit Maintenance Entries
CREATE TRIGGER dbo.TR_MaintenanceEntries_Audit
ON dbo.MaintenanceEntries
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        IF EXISTS (SELECT * FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'UPDATE', GETDATE(), 'MaintenanceEntries', Id FROM inserted;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'CREATE', GETDATE(), 'MaintenanceEntries', Id FROM inserted;
        END
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        -- DELETE
        INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'DELETE', GETDATE(), 'MaintenanceEntries', Id FROM deleted;
    END
END;
GO

-- Trigger: Audit Finance Entries
CREATE TRIGGER dbo.TR_FinanceEntries_Audit
ON dbo.FinanceEntries
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    IF EXISTS (SELECT * FROM inserted)
    BEGIN
        IF EXISTS (SELECT * FROM deleted)
        BEGIN
            -- UPDATE
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'UPDATE', GETDATE(), 'FinanceEntries', Id FROM inserted;
        END
        ELSE
        BEGIN
            -- INSERT
            INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'CREATE', GETDATE(), 'FinanceEntries', Id FROM inserted;
        END
    END
    ELSE IF EXISTS (SELECT * FROM deleted)
    BEGIN
        -- DELETE
        INSERT INTO dbo.AuditLogs (UserId, Action, ActionDate, TableName, RecordId)
            SELECT 'SYSTEM', 'DELETE', GETDATE(), 'FinanceEntries', Id FROM deleted;
    END
END;
GO

-- =============================================
-- SAMPLE QUERIES FOR TESTING
-- =============================================

-- Query 1: Get all vehicles with their current status
SELECT * FROM dbo.VehicleSummary ORDER BY PlateNumber;
GO

-- Query 2: Get fuel expenses for January 2026
SELECT * FROM dbo.FuelExpensesSummary 
WHERE CAST(DateTime AS DATE) BETWEEN '2026-01-01' AND '2026-01-31'
ORDER BY DateTime DESC;
GO

-- Query 3: Get maintenance schedule for next 30 days
EXEC dbo.GetMaintenanceSchedule @DaysAhead = 30;
GO

-- Query 4: Get fuel efficiency report for all vehicles
EXEC dbo.GetFuelEfficiencyReport;
GO

-- Query 5: Get expenses by date range for specific vehicle
EXEC dbo.GetVehicleExpensesByDateRange 
    @StartDate = '2026-01-01', 
    @EndDate = '2026-01-31', 
    @VehicleId = 1;
GO

-- Query 6: Finance summary by vehicle
SELECT * FROM dbo.FinanceSummaryByVehicle ORDER BY TotalCost DESC;
GO

-- =============================================
-- COMPLETION MESSAGE
-- =============================================
PRINT 'RouteX Database Schema created successfully!';
PRINT 'Tables: Vehicles, Users, AuditLogs, FuelEntries, MaintenanceEntries, FinanceEntries';
PRINT 'Views: VehicleSummary, FuelExpensesSummary, MaintenanceSummary, FinanceSummaryByVehicle';
PRINT 'Stored Procedures: GetVehicleExpensesByDateRange, GetMaintenanceSchedule, GetFuelEfficiencyReport';
PRINT 'Triggers: TR_FuelEntries_Audit, TR_MaintenanceEntries_Audit, TR_FinanceEntries_Audit';
PRINT 'Sample data has been inserted for testing purposes.';
PRINT 'All SQL Server batch issues have been resolved with proper GO statements.';
GO
