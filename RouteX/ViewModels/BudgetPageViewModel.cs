using RouteX.Models;

namespace RouteX.ViewModels
{
    public class BudgetPageViewModel
    {
        public int? MonthValue { get; set; }
        public int? Year { get; set; }
        public decimal? Amount { get; set; }
        public List<BudgetEntry> Entries { get; set; } = new();
    }
}
