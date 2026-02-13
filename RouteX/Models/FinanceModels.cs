namespace RouteX.Models
{
    public class FinanceViewModel
    {
        public FinanceSummary Summary { get; set; } = new();
        public List<VehicleCostSummary> CostPerVehicle { get; set; } = new();
        public List<ExpenseRecord> ExpenseRecords { get; set; } = new();
    }

    public class FinanceSummary
    {
        public decimal TotalExpensesAllTime { get; set; }
        public decimal TotalFuelCost { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        public decimal AverageCostPerVehicle { get; set; }
        public string HighestCostVehicle { get; set; } = string.Empty;
    }

    public class VehicleCostSummary
    {
        public string Vehicle { get; set; } = string.Empty;
        public decimal FuelCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal OtherExpenses { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class ExpenseRecord
    {
        public int ExpenseId { get; set; }
        public string Vehicle { get; set; } = string.Empty;
        public string ExpenseType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    public class FuelTimeSeriesData
    {
        public string Period { get; set; } = string.Empty;
        public double FuelCost { get; set; }
    }

    public class MonthlyExpenseBreakdownData
    {
        public string Month { get; set; } = string.Empty;
        public double FuelCost { get; set; }
        public double MaintenanceCost { get; set; }
        public double OtherCost { get; set; }
        public double TotalCost { get; set; }
    }
}
