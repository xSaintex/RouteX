namespace RouteX.Models
{
    public class ExpenseTimeSeriesData
    {
        public string Month { get; set; } = string.Empty;
        public double ExpenseAmount { get; set; }
        public double FuelCost { get; set; }
        public double MaintenanceCost { get; set; }
        public double OtherCost { get; set; }
    }
}
