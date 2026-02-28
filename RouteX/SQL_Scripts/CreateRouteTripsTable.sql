-- Create RouteTrips table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RouteTrips')
BEGIN
    CREATE TABLE [dbo].[RouteTrips] (
        [Id] INT IDENTITY (1,1) NOT NULL,
        [VehicleId] INT NOT NULL,
        [StartAddress] NVARCHAR (256) NOT NULL,
        [EndAddress] NVARCHAR (256) NOT NULL,
        [Status] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CompletedAt] DATETIME2 NULL,
        [CancelledAt] DATETIME2 NULL,
        CONSTRAINT [PK_RouteTrips] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RouteTrips_Vehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [dbo].[Vehicles] ([VehicleId]) ON DELETE NO ACTION
    );
    
    CREATE INDEX [IX_RouteTrips_VehicleId] ON [dbo].[RouteTrips] ([VehicleId]);
END

-- Mark migration as applied
IF NOT EXISTS (SELECT * FROM [__EFMigrationsHistory] WHERE MigrationId = '20260228061739_AddRouteTrips')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20260228061739_AddRouteTrips', '10.0.3');
END
