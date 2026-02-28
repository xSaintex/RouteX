namespace RouteX.ViewModels
{
    public class FinanceDashboardViewModel
    {
        public List<RouteX.Controllers.FuelConsumptionPoint> FuelConsumptionData { get; set; } = new();
        public List<RecentExpenseItem> RecentExpenses { get; set; } = new();
        public List<CostComparisonItem> CostComparison { get; set; } = new();
    }

    public class RecentExpenseItem
    {
        public string Category { get; set; } = string.Empty;
        public string Vehicle { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string? Description { get; set; }
    }

    public class CostComparisonItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
