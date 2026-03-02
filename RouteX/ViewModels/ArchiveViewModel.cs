using RouteX.Models;

namespace RouteX.ViewModels
{
    public class ArchiveViewModel
    {
        public List<Vehicle> ArchivedVehicles { get; set; } = new();
        public List<FuelEntry> ArchivedFuel { get; set; } = new();
        public List<MaintenanceEntry> ArchivedMaintenance { get; set; } = new();
        public List<FinanceEntry> ArchivedFinance { get; set; } = new();
        public List<User> ArchivedUsers { get; set; } = new();
        public List<RouteTrip> ArchivedTrips { get; set; } = new();
        public List<ArchiveItemViewModel> ArchiveItems { get; set; } = new();
    }

    public class ArchiveItemViewModel
    {
        public string Category { get; set; } = string.Empty;
        public string DetailsHtml { get; set; } = string.Empty;
        public string RestoreAction { get; set; } = string.Empty;
        public int RestoreId { get; set; }
        public DateTime ArchivedDate { get; set; }
        public string ArchivedBy { get; set; } = "System";
    }
}
