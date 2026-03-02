using System.Text.Json;

namespace RouteX.Services
{
    public interface IRouteDistanceService
    {
        Task<decimal?> GetDistanceKmAsync(string startAddress, string endAddress, CancellationToken cancellationToken = default);
    }

    public class OpenRouteService : IRouteDistanceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenRouteService> _logger;
        private readonly string _apiKey;

        public OpenRouteService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<OpenRouteService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _apiKey = configuration["OpenRouteService:ApiKey"] ?? string.Empty;
        }

        public async Task<decimal?> GetDistanceKmAsync(string startAddress, string endAddress, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(startAddress) || string.IsNullOrWhiteSpace(endAddress))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                _logger.LogWarning("OpenRouteService API key is not configured.");
                return null;
            }

            try
            {
                _logger.LogInformation("Attempting to get distance from '{Start}' to '{End}'", startAddress, endAddress);
                
                var startCoordinates = await GetCoordinatesAsync(startAddress, cancellationToken);
                var endCoordinates = await GetCoordinatesAsync(endAddress, cancellationToken);

                if (startCoordinates == null || endCoordinates == null)
                {
                    _logger.LogWarning("Unable to resolve coordinates for route from '{Start}' to '{End}'.", startAddress, endAddress);
                    return null;
                }

                var routeUrl =
                    $"https://api.openrouteservice.org/v2/directions/driving-car?api_key={_apiKey}&start={startCoordinates.Value.Longitude},{startCoordinates.Value.Latitude}&end={endCoordinates.Value.Longitude},{endCoordinates.Value.Latitude}";

                _logger.LogInformation("Making OpenRouteService request: {Url}", routeUrl);

                var response = await _httpClient.GetAsync(routeUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenRouteService route request failed with status {StatusCode}.", response.StatusCode);
                    return null;
                }

                var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("OpenRouteService response: {Response}", jsonContent);
                
                var routeData = JsonSerializer.Deserialize<OpenRouteDirectionsResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var distanceMeters = routeData?.Features?.FirstOrDefault()?.Properties?.Summary?.Distance;
                if (distanceMeters == null)
                {
                    _logger.LogWarning("OpenRouteService returned no distance data for route.");
                    return null;
                }

                var distanceKm = (decimal)distanceMeters.Value / 1000m;
                _logger.LogInformation("Successfully calculated distance: {Distance} km", distanceKm);
                return Math.Round(distanceKm, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating distance using OpenRouteService.");
                return null;
            }
        }

        private async Task<Coordinates?> GetCoordinatesAsync(string address, CancellationToken cancellationToken)
        {
            var geocodeUrl =
                $"https://api.openrouteservice.org/geocode/search?api_key={_apiKey}&text={Uri.EscapeDataString(address)}&size=1";

            _logger.LogInformation("Making geocode request for address: {Address}", address);
            _logger.LogInformation("Geocode URL: {Url}", geocodeUrl);

            var response = await _httpClient.GetAsync(geocodeUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenRouteService geocode request failed with status {StatusCode}.", response.StatusCode);
                return null;
            }

            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Geocode response: {Response}", jsonContent);
            
            var geocodeData = JsonSerializer.Deserialize<OpenRouteGeocodeResponse>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var coordinates = geocodeData?.Features?.FirstOrDefault()?.Geometry?.Coordinates;
            if (coordinates == null || coordinates.Length < 2)
            {
                _logger.LogWarning("No coordinates found in geocode response");
                return null;
            }

            var result = new Coordinates(coordinates[1], coordinates[0]);
            _logger.LogInformation("Geocoded coordinates: {Lat}, {Lng}", result.Latitude, result.Longitude);
            return result;
        }

        private readonly record struct Coordinates(double Latitude, double Longitude);
    }

    public class OpenRouteDirectionsResponse
    {
        public List<OpenRouteDirectionsFeature>? Features { get; set; }
    }

    public class OpenRouteDirectionsFeature
    {
        public OpenRouteDirectionsProperties? Properties { get; set; }
    }

    public class OpenRouteDirectionsProperties
    {
        public OpenRouteSummary? Summary { get; set; }
    }

    public class OpenRouteSummary
    {
        public double? Distance { get; set; }
    }

    public class OpenRouteGeocodeResponse
    {
        public List<OpenRouteGeocodeFeature>? Features { get; set; }
    }

    public class OpenRouteGeocodeFeature
    {
        public OpenRouteGeometry? Geometry { get; set; }
    }

    public class OpenRouteGeometry
    {
        public double[]? Coordinates { get; set; }
    }
}
