-- SQL Script to fix the AuditLogs table by adding a primary key
-- Run this script in SQL Server Management Studio or use SQL command

-- Check if table exists and has no primary key
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    -- Drop the existing table (WARNING: This will delete all data)
    DROP TABLE AuditLogs;
    
    -- Create the table with primary key
    CREATE TABLE AuditLogs (
        AuditLogId INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
        UserId NVARCHAR(MAX) NOT NULL,
        Action NVARCHAR(MAX) NOT NULL,
        ActionDate DATETIME2 NOT NULL
    );
    
    PRINT 'AuditLogs table recreated successfully with primary key';
END
ELSE
BEGIN
    PRINT 'AuditLogs table does not exist';
END
