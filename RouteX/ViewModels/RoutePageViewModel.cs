using RouteX.Models;

namespace RouteX.ViewModels
{
    public class RoutePageViewModel
    {
        public Vehicle Vehicle { get; set; } = null!;
        public RouteTrip? CurrentRoute { get; set; }
        public List<RouteTrip> TripHistory { get; set; } = new();
    }
}
