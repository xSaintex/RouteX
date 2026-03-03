-- Apply pending database updates for RouteX after publishing
-- This script adds the vehicle approval workflow columns and marks migrations as applied

USE [db41235];
GO

-- Add Vehicle Approval Workflow columns if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'AddedByUserEmail')
BEGIN
    ALTER TABLE Vehicles ADD AddedByUserEmail NVARCHAR(MAX) NULL;
    PRINT 'Added AddedByUserEmail column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'AddedByUserId')
BEGIN
    ALTER TABLE Vehicles ADD AddedByUserId NVARCHAR(MAX) NULL;
    PRINT 'Added AddedByUserId column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'ApprovalDate')
BEGIN
    ALTER TABLE Vehicles ADD ApprovalDate DATETIME2 NULL;
    PRINT 'Added ApprovalDate column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'ApprovedByUserEmail')
BEGIN
    ALTER TABLE Vehicles ADD ApprovedByUserEmail NVARCHAR(MAX) NULL;
    PRINT 'Added ApprovedByUserEmail column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'ApprovedByUserId')
BEGIN
    ALTER TABLE Vehicles ADD ApprovedByUserId NVARCHAR(MAX) NULL;
    PRINT 'Added ApprovedByUserId column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'CreatedDate')
BEGIN
    ALTER TABLE Vehicles ADD CreatedDate DATETIME2 NULL;
    PRINT 'Added CreatedDate column to Vehicles table';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Vehicles') AND name = 'IsPendingApproval')
BEGIN
    ALTER TABLE Vehicles ADD IsPendingApproval BIT NOT NULL DEFAULT 0;
    PRINT 'Added IsPendingApproval column to Vehicles table';
END

-- Mark migrations as applied in __EFMigrationsHistory table
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260220145942_InitialCreate')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260220145942_InitialCreate', '10.0.3');
    PRINT 'Marked InitialCreate migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260228061739_AddRouteTrips')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260228061739_AddRouteTrips', '10.0.3');
    PRINT 'Marked AddRouteTrips migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260228072624_AddBranchesSystem')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260228072624_AddBranchesSystem', '10.0.3');
    PRINT 'Marked AddBranchesSystem migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260301000000_AddRouteTripDistanceKm')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260301000000_AddRouteTripDistanceKm', '10.0.3');
    PRINT 'Marked AddRouteTripDistanceKm migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260301130510_RemoveMileageFromVehicle')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260301130510_RemoveMileageFromVehicle', '10.0.3');
    PRINT 'Marked RemoveMileageFromVehicle migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260301155658_AddArchivePropertiesToRouteTrip')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260301155658_AddArchivePropertiesToRouteTrip', '10.0.3');
    PRINT 'Marked AddArchivePropertiesToRouteTrip migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260301183408_AddTotalMileageToVehicle')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260301183408_AddTotalMileageToVehicle', '10.0.3');
    PRINT 'Marked AddTotalMileageToVehicle migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260302045921_PendingModelChanges')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260302045921_PendingModelChanges', '10.0.3');
    PRINT 'Marked PendingModelChanges migration as applied';
END

IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20260302125120_AddVehicleApprovalWorkflow')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
    VALUES ('20260302125120_AddVehicleApprovalWorkflow', '10.0.3');
    PRINT 'Marked AddVehicleApprovalWorkflow migration as applied';
END

PRINT 'Database update completed successfully!';
