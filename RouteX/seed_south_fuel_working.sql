-- Seed South Branch Fuel Entries (100+ entries from Jan 2025 to Mar 2026)

-- Insert fuel entries for each South Branch vehicle
INSERT INTO FuelEntries (VehicleId, Driver, DateTime, FuelStation, Odometer, Liters, TotalCost, FuelType, FullTank, Notes, UnitModel, PlateNumber, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
VALUES 
-- SBR-1001 Toyota Vios
(53, 'Juan Santos', '2025-01-15 08:30:00', 'Shell', 12000, 35.50, 2582.75, 'Diesel', 1, 'Regular maintenance refuel', 'Toyota Vios', 'SBR-1001', '2025-01-15', GETDATE(), GETDATE(), 0, 3),
(53, 'Juan Santos', '2025-02-20 14:15:00', 'Caltex', 14500, 42.30, 3074.65, 'Diesel', 1, 'Long trip preparation', 'Toyota Vios', 'SBR-1001', '2025-02-20', GETDATE(), GETDATE(), 0, 3),
(53, 'Juan Santos', '2025-03-10 09:45:00', 'Petron', 15200, 28.80, 2094.24, 'Diesel', 0, 'Emergency fuel stop', 'Toyota Vios', 'SBR-1001', '2025-03-10', GETDATE(), GETDATE(), 0, 3),

-- SBR-1002 Honda Civic
(54, 'Maria Reyes', '2025-01-22 11:00:00', 'Seaoil', 21000, 38.20, 2783.96, 'Premium', 1, 'Weekly fueling schedule', 'Honda Civic', 'SBR-1002', '2025-01-22', GETDATE(), GETDATE(), 0, 3),
(54, 'Maria Reyes', '2025-02-14 16:30:00', 'Phoenix', 23500, 45.60, 3320.48, 'Premium', 1, 'Company vehicle refueling', 'Honda Civic', 'SBR-1002', '2025-02-14', GETDATE(), GETDATE(), 0, 3),
(54, 'Maria Reyes', '2025-03-05 10:20:00', 'Cleanoil', 25800, 33.40, 2431.52, 'Premium', 0, 'Monthly fuel report', 'Honda Civic', 'SBR-1002', '2025-03-05', GETDATE(), GETDATE(), 0, 3),

-- SBR-1003 Mitsubishi Mirage
(55, 'Carlos Cruz', '2025-01-18 07:45:00', 'Caltex', 6500, 32.10, 2338.05, 'Regular', 1, 'Special delivery fueling', 'Mitsubishi Mirage', 'SBR-1003', '2025-01-18', GETDATE(), GETDATE(), 0, 3),
(55, 'Carlos Cruz', '2025-02-25 13:45:00', 'Shell', 8000, 38.90, 2682.21, 'Regular', 1, 'Weekend trip fuel', 'Mitsubishi Mirage', 'SBR-1003', '2025-02-25', GETDATE(), GETDATE(), 0, 3),
(55, 'Carlos Cruz', '2025-03-12 15:30:00', 'Petron', 8800, 29.50, 2032.55, 'Regular', 0, 'Emergency backup fuel', 'Mitsubishi Mirage', 'SBR-1003', '2025-03-12', GETDATE(), GETDATE(), 0, 3),

-- SBR-1004 Nissan Sentra
(56, 'Ana Martinez', '2025-01-10 09:15:00', 'Seaoil', 28000, 40.50, 2789.85, 'Unleaded', 1, 'Standard company refuel', 'Nissan Sentra', 'SBR-1004', '2025-01-10', GETDATE(), GETDATE(), 0, 3),
(56, 'Ana Martinez', '2025-02-18 12:30:00', 'Phoenix', 30500, 36.80, 2620.16, 'Unleaded', 1, 'Regular maintenance refuel', 'Nissan Sentra', 'SBR-1004', '2025-02-18', GETDATE(), GETDATE(), 0, 3),
(56, 'Ana Martinez', '2025-03-08 14:20:00', 'Cleanoil', 32800, 42.10, 2997.52, 'Unleaded', 1, 'Long trip preparation', 'Nissan Sentra', 'SBR-1004', '2025-03-08', GETDATE(), GETDATE(), 0, 3),

-- SBR-1005 Ford Ranger
(57, 'Roberto Garcia', '2025-01-12 08:00:00', 'Caltex', 41000, 48.50, 3176.75, 'Diesel', 1, 'Heavy duty refuel', 'Ford Ranger', 'SBR-1005', '2025-01-12', GETDATE(), GETDATE(), 0, 3),
(57, 'Roberto Garcia', '2025-02-22 15:45:00', 'Shell', 43500, 52.30, 3425.65, 'Diesel', 1, 'Construction site fueling', 'Ford Ranger', 'SBR-1005', '2025-02-22', GETDATE(), GETDATE(), 0, 3),
(57, 'Roberto Garcia', '2025-03-15 11:30:00', 'Petron', 45200, 44.80, 2934.40, 'Diesel', 0, 'Emergency fuel stop', 'Ford Ranger', 'SBR-1005', '2025-03-15', GETDATE(), GETDATE(), 0, 3),

-- SBR-1006 Isuzu D-Max
(58, 'Elena Rodriguez', '2025-01-20 10:30:00', 'Seaoil', 24000, 46.20, 3026.10, 'Diesel', 1, 'Monthly fuel report', 'Isuzu D-Max', 'SBR-1006', '2025-01-20', GETDATE(), GETDATE(), 0, 3),
(58, 'Elena Rodriguez', '2025-02-28 13:15:00', 'Phoenix', 26500, 39.80, 2606.90, 'Diesel', 1, 'Company vehicle refueling', 'Isuzu D-Max', 'SBR-1006', '2025-02-28', GETDATE(), GETDATE(), 0, 3),
(58, 'Elena Rodriguez', '2025-03-18 16:45:00', 'Cleanoil', 28700, 41.50, 2718.25, 'Diesel', 1, 'Weekend trip fuel', 'Isuzu D-Max', 'SBR-1006', '2025-03-18', GETDATE(), GETDATE(), 0, 3),

-- SBR-1007 Toyota Hilux
(59, 'Juan Santos', '2025-01-25 09:00:00', 'Caltex', 47000, 50.20, 3288.10, 'Diesel', 1, 'Heavy load fueling', 'Toyota Hilux', 'SBR-1007', '2025-01-25', GETDATE(), GETDATE(), 0, 3),
(59, 'Juan Santos', '2025-03-01 14:20:00', 'Shell', 49500, 47.80, 3130.90, 'Diesel', 1, 'Long distance trip', 'Toyota Hilux', 'SBR-1007', '2025-03-01', GETDATE(), GETDATE(), 0, 3),
(59, 'Juan Santos', '2025-03-20 11:15:00', 'Petron', 51000, 43.60, 2855.80, 'Diesel', 0, 'Emergency refuel', 'Toyota Hilux', 'SBR-1007', '2025-03-20', GETDATE(), GETDATE(), 0, 3),

-- SBR-1008 Honda CR-V
(60, 'Maria Reyes', '2025-01-08 12:45:00', 'Seaoil', 15000, 35.80, 2606.24, 'Premium', 1, 'Family trip fueling', 'Honda CR-V', 'SBR-1008', '2025-01-08', GETDATE(), GETDATE(), 0, 3),
(60, 'Maria Reyes', '2025-02-16 09:30:00', 'Phoenix', 17500, 40.20, 2926.56, 'Premium', 1, 'Company vehicle refueling', 'Honda CR-V', 'SBR-1008', '2025-02-16', GETDATE(), GETDATE(), 0, 3),
(60, 'Maria Reyes', '2025-03-10 15:10:00', 'Cleanoil', 19800, 37.40, 2722.72, 'Premium', 1, 'Monthly maintenance fuel', 'Honda CR-V', 'SBR-1008', '2025-03-10', GETDATE(), GETDATE(), 0, 3),

-- SBR-1009 Mitsubishi Xpander
(61, 'Carlos Cruz', '2025-01-30 08:15:00', 'Caltex', 8000, 33.50, 2308.15, 'Regular', 1, 'New vehicle fueling', 'Mitsubishi Xpander', 'SBR-1009', '2025-01-30', GETDATE(), GETDATE(), 0, 3),
(61, 'Carlos Cruz', '2025-02-24 13:40:00', 'Shell', 10500, 38.90, 2682.21, 'Regular', 1, 'Company trip fueling', 'Mitsubishi Xpander', 'SBR-1009', '2025-02-24', GETDATE(), GETDATE(), 0, 3),
(61, 'Carlos Cruz', '2025-03-14 10:25:00', 'Petron', 12300, 35.20, 2425.28, 'Regular', 0, 'Emergency fuel stop', 'Mitsubishi Xpander', 'SBR-1009', '2025-03-14', GETDATE(), GETDATE(), 0, 3),

-- SBR-1010 Nissan Navara
(62, 'Ana Martinez', '2025-01-15 11:30:00', 'Seaoil', 35000, 45.60, 3244.32, 'Diesel', 1, 'Heavy duty refuel', 'Nissan Navara', 'SBR-1010', '2025-01-15', GETDATE(), GETDATE(), 0, 3),
(62, 'Ana Martinez', '2025-02-20 14:50:00', 'Phoenix', 37500, 42.30, 3025.65, 'Diesel', 1, 'Construction site fueling', 'Nissan Navara', 'SBR-1010', '2025-02-20', GETDATE(), GETDATE(), 0, 3),
(62, 'Ana Martinez', '2025-03-12 09:15:00', 'Cleanoil', 38700, 38.80, 2778.40, 'Diesel', 0, 'Emergency backup fuel', 'Nissan Navara', 'SBR-1010', '2025-03-12', GETDATE(), GETDATE(), 0, 3),

-- SBR-1011 Ford Everest
(63, 'Roberto Garcia', '2025-01-22 08:45:00', 'Caltex', 38000, 48.20, 3157.10, 'Diesel', 1, 'Family trip fueling', 'Ford Everest', 'SBR-1011', '2025-01-22', GETDATE(), GETDATE(), 0, 3),
(63, 'Roberto Garcia', '2025-03-05 12:20:00', 'Shell', 40500, 44.60, 2921.30, 'Diesel', 1, 'Company vehicle refueling', 'Ford Everest', 'SBR-1011', '2025-03-05', GETDATE(), GETDATE(), 0, 3),
(63, 'Roberto Garcia', '2025-03-18 15:35:00', 'Petron', 42100, 41.30, 2705.15, 'Diesel', 0, 'Emergency refuel', 'Ford Everest', 'SBR-1011', '2025-03-18', GETDATE(), GETDATE(), 0, 3),

-- SBR-1012 Isuzu Mu-X
(64, 'Elena Rodriguez', '2025-01-18 10:15:00', 'Seaoil', 31000, 46.80, 3065.40, 'Diesel', 1, 'Monthly fuel report', 'Isuzu Mu-X', 'SBR-1012', '2025-01-18', GETDATE(), GETDATE(), 0, 3),
(64, 'Elena Rodriguez', '2025-02-27 14:30:00', 'Phoenix', 33500, 43.20, 2829.60, 'Diesel', 1, 'Company vehicle refueling', 'Isuzu Mu-X', 'SBR-1012', '2025-02-27', GETDATE(), GETDATE(), 0, 3),
(64, 'Elena Rodriguez', '2025-03-15 11:45:00', 'Cleanoil', 35600, 39.80, 2606.90, 'Diesel', 0, 'Emergency fuel stop', 'Isuzu Mu-X', 'SBR-1012', '2025-03-15', GETDATE(), GETDATE(), 0, 3),

-- SBR-1013 Toyota Innova
(65, 'Juan Santos', '2025-01-25 09:30:00', 'Caltex', 23000, 42.10, 2901.69, 'Regular', 1, 'Family trip fueling', 'Toyota Innova', 'SBR-1013', '2025-01-25', GETDATE(), GETDATE(), 0, 3),
(65, 'Juan Santos', '2025-03-02 13:15:00', 'Shell', 25500, 38.50, 2652.15, 'Regular', 1, 'Company vehicle refueling', 'Toyota Innova', 'SBR-1013', '2025-03-02', GETDATE(), GETDATE(), 0, 3),
(65, 'Juan Santos', '2025-03-20 16:20:00', 'Petron', 27800, 35.90, 2473.51, 'Regular', 0, 'Emergency refuel', 'Toyota Innova', 'SBR-1013', '2025-03-20', GETDATE(), GETDATE(), 0, 3),

-- SBR-1014 Honda City
(66, 'Maria Reyes', '2025-01-12 11:00:00', 'Seaoil', 12000, 34.60, 2382.76, 'Premium', 1, 'Daily commute fueling', 'Honda City', 'SBR-1014', '2025-01-12', GETDATE(), GETDATE(), 0, 3),
(66, 'Maria Reyes', '2025-02-19 15:25:00', 'Phoenix', 14500, 40.80, 2970.24, 'Premium', 1, 'Company vehicle refueling', 'Honda City', 'SBR-1014', '2025-02-19', GETDATE(), GETDATE(), 0, 3),
(66, 'Maria Reyes', '2025-03-08 10:40:00', 'Cleanoil', 16500, 37.20, 2708.16, 'Premium', 0, 'Emergency fuel stop', 'Honda City', 'SBR-1014', '2025-03-08', GETDATE(), GETDATE(), 0, 3),

-- SBR-1015 Mitsubishi L300
(67, 'Carlos Cruz', '2025-01-20 08:20:00', 'Caltex', 48000, 52.30, 3605.85, 'Diesel', 1, 'Heavy load fueling', 'Mitsubishi L300', 'SBR-1015', '2025-01-20', GETDATE(), GETDATE(), 0, 3),
(67, 'Carlos Cruz', '2025-03-01 14:10:00', 'Shell', 50500, 48.70, 3189.85, 'Diesel', 1, 'Delivery trip fueling', 'Mitsubishi L300', 'SBR-1015', '2025-03-01', GETDATE(), GETDATE(), 0, 3),
(67, 'Carlos Cruz', '2025-03-17 11:50:00', 'Petron', 52300, 45.20, 2960.60, 'Diesel', 0, 'Emergency backup fuel', 'Mitsubishi L300', 'SBR-1015', '2025-03-17', GETDATE(), GETDATE(), 0, 3),

-- SBR-1016 Nissan Urvan
(68, 'Ana Martinez', '2025-01-28 09:45:00', 'Seaoil', 43000, 49.80, 3432.36, 'Diesel', 1, 'Passenger van fueling', 'Nissan Urvan', 'SBR-1016', '2025-01-28', GETDATE(), GETDATE(), 0, 3),
(68, 'Ana Martinez', '2025-03-06 13:30:00', 'Phoenix', 46000, 46.20, 3026.10, 'Diesel', 1, 'Company trip fueling', 'Nissan Urvan', 'SBR-1016', '2025-03-06', GETDATE(), GETDATE(), 0, 3),
(68, 'Ana Martinez', '2025-03-19 16:15:00', 'Cleanoil', 48900, 42.80, 2803.40, 'Diesel', 0, 'Emergency refuel', 'Nissan Urvan', 'SBR-1016', '2025-03-19', GETDATE(), GETDATE(), 0, 3),

-- SBR-1017 Ford Transit
(69, 'Roberto Garcia', '2025-01-15 10:30:00', 'Caltex', 37000, 47.50, 3113.75, 'Diesel', 1, 'Cargo van fueling', 'Ford Transit', 'SBR-1017', '2025-01-15', GETDATE(), GETDATE(), 0, 3),
(69, 'Roberto Garcia', '2025-02-28 15:45:00', 'Shell', 39500, 44.10, 2888.55, 'Diesel', 1, 'Delivery trip fueling', 'Ford Transit', 'SBR-1017', '2025-02-28', GETDATE(), GETDATE(), 0, 3),
(69, 'Roberto Garcia', '2025-03-14 12:20:00', 'Petron', 41200, 40.80, 2672.40, 'Diesel', 0, 'Emergency fuel stop', 'Ford Transit', 'SBR-1017', '2025-03-14', GETDATE(), GETDATE(), 0, 3),

-- SBR-1018 Isuzu Crosswind
(70, 'Elena Rodriguez', '2025-01-22 08:15:00', 'Seaoil', 32000, 45.60, 2987.40, 'Diesel', 1, 'Monthly fuel report', 'Isuzu Crosswind', 'SBR-1018', '2025-01-22', GETDATE(), GETDATE(), 0, 3),
(70, 'Elena Rodriguez', '2025-03-03 14:25:00', 'Phoenix', 34500, 42.30, 2770.65, 'Diesel', 1, 'Company vehicle refueling', 'Isuzu Crosswind', 'SBR-1018', '2025-03-03', GETDATE(), GETDATE(), 0, 3),
(70, 'Elena Rodriguez', '2025-03-18 11:10:00', 'Cleanoil', 36700, 38.90, 2547.85, 'Diesel', 0, 'Emergency backup fuel', 'Isuzu Crosswind', 'SBR-1018', '2025-03-18', GETDATE(), GETDATE(), 0, 3),

-- SBR-1019 Toyota Avanza
(71, 'Juan Santos', '2025-01-10 11:20:00', 'Caltex', 17000, 36.80, 2535.52, 'Regular', 1, 'Family trip fueling', 'Toyota Avanza', 'SBR-1019', '2025-01-10', GETDATE(), GETDATE(), 0, 3),
(71, 'Juan Santos', '2025-02-25 16:40:00', 'Shell', 19500, 40.20, 2770.78, 'Regular', 1, 'Company vehicle refueling', 'Toyota Avanza', 'SBR-1019', '2025-02-25', GETDATE(), GETDATE(), 0, 3),
(71, 'Juan Santos', '2025-03-12 09:55:00', 'Petron', 21300, 37.60, 2592.64, 'Regular', 0, 'Emergency refuel', 'Toyota Avanza', 'SBR-1019', '2025-03-12', GETDATE(), GETDATE(), 0, 3),

-- SBR-1020 Honda Jazz
(72, 'Maria Reyes', '2025-01-18 12:10:00', 'Seaoil', 10000, 32.50, 2240.25, 'Premium', 1, 'Daily commute fueling', 'Honda Jazz', 'SBR-1020', '2025-01-18', GETDATE(), GETDATE(), 0, 3),
(72, 'Maria Reyes', '2025-02-20 14:35:00', 'Phoenix', 12500, 38.80, 2824.64, 'Premium', 1, 'Company vehicle refueling', 'Honda Jazz', 'SBR-1020', '2025-02-20', GETDATE(), GETDATE(), 0, 3),
(72, 'Maria Reyes', '2025-03-09 10:45:00', 'Cleanoil', 14200, 35.20, 2562.56, 'Premium', 0, 'Emergency fuel stop', 'Honda Jazz', 'SBR-1020', '2025-03-09', GETDATE(), GETDATE(), 0, 3),

-- SBR-2001 Toyota Corolla (Inactive)
(73, 'Carlos Cruz', '2025-01-05 09:00:00', 'Caltex', 85000, 44.60, 3072.74, 'Regular', 1, 'Last active fueling', 'Toyota Corolla', 'SBR-2001', '2025-01-05', GETDATE(), GETDATE(), 0, 3),
(73, 'Carlos Cruz', '2025-01-20 13:15:00', 'Shell', 87000, 41.20, 2838.68, 'Regular', 1, 'Pre-maintenance fueling', 'Toyota Corolla', 'SBR-2001', '2025-01-20', GETDATE(), GETDATE(), 0, 3),
(73, 'Carlos Cruz', '2025-02-10 11:30:00', 'Petron', 89000, 38.50, 2651.65, 'Regular', 0, 'Final fueling before inactive', 'Toyota Corolla', 'SBR-2001', '2025-02-10', GETDATE(), GETDATE(), 0, 3),

-- SBR-2002 Honda Accord (Inactive)
(74, 'Ana Martinez', '2025-01-08 10:45:00', 'Seaoil', 73000, 42.30, 2914.47, 'Premium', 1, 'Regular maintenance fueling', 'Honda Accord', 'SBR-2002', '2025-01-08', GETDATE(), GETDATE(), 0, 3),
(74, 'Ana Martinez', '2025-01-25 15:20:00', 'Phoenix', 75000, 39.80, 2898.44, 'Premium', 1, 'Company vehicle refueling', 'Honda Accord', 'SBR-2002', '2025-01-25', GETDATE(), GETDATE(), 0, 3),
(74, 'Ana Martinez', '2025-02-15 12:50:00', 'Cleanoil', 76500, 36.40, 2649.92, 'Premium', 0, 'Final fueling before inactive', 'Honda Accord', 'SBR-2002', '2025-02-15', GETDATE(), GETDATE(), 0, 3),

-- SBR-2003 Mitsubishi Lancer (Inactive)
(75, 'Roberto Garcia', '2025-01-12 08:30:00', 'Caltex', 88000, 46.80, 3222.72, 'Regular', 1, 'Monthly fuel report', 'Mitsubishi Lancer', 'SBR-2003', '2025-01-12', GETDATE(), GETDATE(), 0, 3),
(75, 'Roberto Garcia', '2025-01-30 14:15:00', 'Shell', 90500, 43.20, 2978.88, 'Regular', 1, 'Company vehicle refueling', 'Mitsubishi Lancer', 'SBR-2003', '2025-01-30', GETDATE(), GETDATE(), 0, 3),
(75, 'Roberto Garcia', '2025-02-20 10:25:00', 'Petron', 92300, 40.60, 2795.34, 'Regular', 0, 'Final fueling before inactive', 'Mitsubishi Lancer', 'SBR-2003', '2025-02-20', GETDATE(), GETDATE(), 0, 3),

-- SBR-2004 Nissan Altima (Inactive)
(76, 'Elena Rodriguez', '2025-01-15 11:15:00', 'Seaoil', 77000, 44.50, 3067.05, 'Premium', 1, 'Regular maintenance fueling', 'Nissan Altima', 'SBR-2004', '2025-01-15', GETDATE(), GETDATE(), 0, 3),
(76, 'Elena Rodriguez', '2025-02-03 16:40:00', 'Phoenix', 79500, 41.10, 2992.08, 'Premium', 1, 'Company vehicle refueling', 'Nissan Altima', 'SBR-2004', '2025-02-03', GETDATE(), GETDATE(), 0, 3),
(76, 'Elena Rodriguez', '2025-02-18 13:55:00', 'Cleanoil', 81400, 38.20, 2782.96, 'Premium', 0, 'Final fueling before inactive', 'Nissan Altima', 'SBR-2004', '2025-02-18', GETDATE(), GETDATE(), 0, 3),

-- SBR-2005 Ford Focus (Inactive)
(77, 'Juan Santos', '2025-01-20 09:20:00', 'Caltex', 64000, 42.80, 2948.24, 'Regular', 1, 'Monthly fuel report', 'Ford Focus', 'SBR-2005', '2025-01-20', GETDATE(), GETDATE(), 0, 3),
(77, 'Juan Santos', '2025-02-08 12:45:00', 'Shell', 66500, 39.40, 2713.86, 'Regular', 1, 'Company vehicle refueling', 'Ford Focus', 'SBR-2005', '2025-02-08', GETDATE(), GETDATE(), 0, 3),
(77, 'Juan Santos', '2025-02-25 10:10:00', 'Petron', 67800, 36.80, 2533.92, 'Regular', 0, 'Final fueling before inactive', 'Ford Focus', 'SBR-2005', '2025-02-25', GETDATE(), GETDATE(), 0, 3),

-- SBR-2006 Isuzu Gemini (Inactive)
(78, 'Maria Reyes', '2025-01-25 08:50:00', 'Seaoil', 91000, 48.20, 3319.78, 'Regular', 1, 'Heavy usage fueling', 'Isuzu Gemini', 'SBR-2006', '2025-01-25', GETDATE(), GETDATE(), 0, 3),
(78, 'Maria Reyes', '2025-02-12 15:30:00', 'Phoenix', 93500, 44.80, 3082.24, 'Regular', 1, 'Company vehicle refueling', 'Isuzu Gemini', 'SBR-2006', '2025-02-12', GETDATE(), GETDATE(), 0, 3),
(78, 'Maria Reyes', '2025-03-01 11:40:00', 'Cleanoil', 95200, 41.60, 2864.24, 'Regular', 0, 'Final fueling before inactive', 'Isuzu Gemini', 'SBR-2006', '2025-03-01', GETDATE(), GETDATE(), 0, 3),

-- SBR-2007 Toyota Camry (Inactive)
(79, 'Carlos Cruz', '2025-01-10 10:00:00', 'Caltex', 85000, 45.60, 3141.84, 'Premium', 1, 'Regular maintenance fueling', 'Toyota Camry', 'SBR-2007', '2025-01-10', GETDATE(), GETDATE(), 0, 3),
(79, 'Carlos Cruz', '2025-01-28 14:20:00', 'Shell', 87500, 42.30, 3081.84, 'Premium', 1, 'Company vehicle refueling', 'Toyota Camry', 'SBR-2007', '2025-01-28', GETDATE(), GETDATE(), 0, 3),
(79, 'Carlos Cruz', '2025-02-15 12:30:00', 'Petron', 88700, 39.80, 2897.44, 'Premium', 0, 'Final fueling before inactive', 'Toyota Camry', 'SBR-2007', '2025-02-15', GETDATE(), GETDATE(), 0, 3),

-- SBR-2008 Honda Prelude (Inactive)
(80, 'Ana Martinez', '2025-01-18 11:30:00', 'Seaoil', 90000, 47.20, 3436.16, 'Premium', 1, 'Monthly fuel report', 'Honda Prelude', 'SBR-2008', '2025-01-18', GETDATE(), GETDATE(), 0, 3),
(80, 'Ana Martinez', '2025-02-05 16:15:00', 'Phoenix', 92500, 43.80, 3188.64, 'Premium', 1, 'Company vehicle refueling', 'Honda Prelude', 'SBR-2008', '2025-02-05', GETDATE(), GETDATE(), 0, 3),
(80, 'Ana Martinez', '2025-02-22 13:45:00', 'Cleanoil', 94600, 40.50, 2948.40, 'Premium', 0, 'Final fueling before inactive', 'Honda Prelude', 'SBR-2008', '2025-02-22', GETDATE(), GETDATE(), 0, 3),

-- SBR-2009 Mitsubishi Galant (Inactive)
(81, 'Roberto Garcia', '2025-01-22 09:15:00', 'Caltex', 68000, 43.60, 3004.04, 'Regular', 1, 'Regular maintenance fueling', 'Mitsubishi Galant', 'SBR-2009', '2025-01-22', GETDATE(), GETDATE(), 0, 3),
(81, 'Roberto Garcia', '2025-02-10 13:50:00', 'Shell', 70500, 40.20, 2770.78, 'Regular', 1, 'Company vehicle refueling', 'Mitsubishi Galant', 'SBR-2009', '2025-02-10', GETDATE(), GETDATE(), 0, 3),
(81, 'Roberto Garcia', '2025-02-28 11:20:00', 'Petron', 72300, 37.80, 2603.22, 'Regular', 0, 'Final fueling before inactive', 'Mitsubishi Galant', 'SBR-2009', '2025-02-28', GETDATE(), GETDATE(), 0, 3),

-- SBR-2010 Nissan Maxima (Inactive)
(82, 'Elena Rodriguez', '2025-01-25 10:45:00', 'Seaoil', 85000, 46.40, 3192.48, 'Premium', 1, 'Monthly fuel report', 'Nissan Maxima', 'SBR-2010', '2025-01-25', GETDATE(), GETDATE(), 0, 3),
(82, 'Elena Rodriguez', '2025-02-15 15:25:00', 'Phoenix', 87500, 43.10, 3137.68, 'Premium', 1, 'Company vehicle refueling', 'Nissan Maxima', 'SBR-2010', '2025-02-15', GETDATE(), GETDATE(), 0, 3),
(82, 'Elena Rodriguez', '2025-03-05 12:15:00', 'Cleanoil', 89500, 39.80, 2897.44, 'Premium', 0, 'Final fueling before inactive', 'Nissan Maxima', 'SBR-2010', '2025-03-05', GETDATE(), GETDATE(), 0, 3);

-- Additional 15 fuel entries for Feb-Mar 2026 (within last 30 days)
INSERT INTO FuelEntries (VehicleId, Driver, DateTime, FuelStation, Odometer, Liters, TotalCost, FuelType, FullTank, Notes, UnitModel, PlateNumber, Date, CreatedDate, ModifiedDate, IsArchived, BranchId)
VALUES 
-- February 2026 entries
(53, 'Juan Santos', '2026-02-15 08:30:00', 'Shell', 15500, 36.50, 2654.75, 'Diesel', 1, 'Recent refuel Feb 2026', 'Toyota Vios', 'SBR-1001', '2026-02-15', GETDATE(), GETDATE(), 0, 3),
(54, 'Maria Reyes', '2026-02-18 14:15:00', 'Caltex', 24000, 40.30, 2933.83, 'Premium', 1, 'Monthly fuel Feb 2026', 'Honda Civic', 'SBR-1002', '2026-02-18', GETDATE(), GETDATE(), 0, 3),
(55, 'Carlos Cruz', '2026-02-20 09:45:00', 'Petron', 9200, 30.80, 2243.24, 'Regular', 0, 'Emergency fuel Feb 2026', 'Mitsubishi Mirage', 'SBR-1003', '2026-02-20', GETDATE(), GETDATE(), 0, 3),
(56, 'Ana Martinez', '2026-02-22 11:00:00', 'Seaoil', 31000, 41.50, 2859.50, 'Unleaded', 1, 'Standard refuel Feb 2026', 'Nissan Sentra', 'SBR-1004', '2026-02-22', GETDATE(), GETDATE(), 0, 3),
(57, 'Roberto Garcia', '2026-02-25 08:00:00', 'Phoenix', 42000, 50.20, 3288.10, 'Diesel', 1, 'Heavy duty Feb 2026', 'Ford Ranger', 'SBR-1005', '2026-02-25', GETDATE(), GETDATE(), 0, 3),
(58, 'Elena Rodriguez', '2026-02-28 10:30:00', 'Cleanoil', 25000, 42.20, 2764.10, 'Diesel', 1, 'Company trip Feb 2026', 'Isuzu D-Max', 'SBR-1006', '2026-02-28', GETDATE(), GETDATE(), 0, 3),

-- March 2026 entries
(59, 'Juan Santos', '2026-03-02 09:00:00', 'Caltex', 48000, 48.50, 3176.75, 'Diesel', 1, 'March delivery fueling', 'Toyota Hilux', 'SBR-1007', '2026-03-02', GETDATE(), GETDATE(), 0, 3),
(60, 'Maria Reyes', '2026-03-05 12:45:00', 'Shell', 16000, 38.80, 2820.80, 'Premium', 1, 'March commute refuel', 'Honda CR-V', 'SBR-1008', '2026-03-05', GETDATE(), GETDATE(), 0, 3),
(61, 'Carlos Cruz', '2026-03-08 08:15:00', 'Petron', 9500, 35.50, 2441.90, 'Regular', 1, 'March work trip', 'Mitsubishi Xpander', 'SBR-1009', '2026-03-08', GETDATE(), GETDATE(), 0, 3),
(62, 'Ana Martinez', '2026-03-10 11:30:00', 'Seaoil', 32000, 44.60, 3175.54, 'Diesel', 1, 'March construction fuel', 'Nissan Navara', 'SBR-1010', '2026-03-10', GETDATE(), GETDATE(), 0, 3),
(63, 'Roberto Garcia', '2026-03-12 08:45:00', 'Phoenix', 39000, 46.20, 3026.10, 'Diesel', 1, 'March family trip', 'Ford Everest', 'SBR-1011', '2026-03-12', GETDATE(), GETDATE(), 0, 3),
(64, 'Elena Rodriguez', '2026-03-14 10:15:00', 'Cleanoil', 26000, 40.50, 2652.75, 'Diesel', 1, 'March maintenance fuel', 'Isuzu Mu-X', 'SBR-1012', '2026-03-14', GETDATE(), GETDATE(), 0, 3),
(65, 'Juan Santos', '2026-03-16 09:30:00', 'Caltex', 23500, 39.80, 2742.22, 'Regular', 1, 'March company fuel', 'Toyota Innova', 'SBR-1013', '2026-03-16', GETDATE(), GETDATE(), 0, 3),
(66, 'Maria Reyes', '2026-03-18 12:10:00', 'Shell', 13000, 36.50, 2512.25, 'Premium', 1, 'March daily fuel', 'Honda City', 'SBR-1014', '2026-03-18', GETDATE(), GETDATE(), 0, 3),
(67, 'Carlos Cruz', '2026-03-20 08:20:00', 'Petron', 49000, 47.80, 3121.70, 'Diesel', 1, 'March heavy load', 'Mitsubishi L300', 'SBR-1015', '2026-03-20', GETDATE(), GETDATE(), 0, 3);

PRINT 'South Branch fuel entries seeding completed!';
PRINT 'Fuel stations used: Caltex, Shell, Petron, Seaoil, Phoenix, Cleanoil';
PRINT 'Fuel types used: Diesel, Premium, Regular, Unleaded';
PRINT 'Date range: January 2025 to March 2026';
PRINT 'Total entries created: 105 (90 original + 15 new Feb-Mar 2026)';
