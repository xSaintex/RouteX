-- Add missing archive columns to RouteTrips table
ALTER TABLE RouteTrips ADD IsArchived BIT NOT NULL DEFAULT 0;
ALTER TABLE RouteTrips ADD ArchivedAt DATETIME2 NULL;
