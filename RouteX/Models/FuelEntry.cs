namespace RouteX.Models
{
    public class FuelEntry
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Driver { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
        public string FuelStation { get; set; } = string.Empty;
        public int Odometer { get; set; }
        public decimal Liters { get; set; }
        public decimal TotalCost { get; set; }
        public string FuelType { get; set; } = string.Empty;
        public bool FullTank { get; set; }
        public string Notes { get; set; } = string.Empty;
        
        // Legacy properties for backward compatibility
        public string UnitModel { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
