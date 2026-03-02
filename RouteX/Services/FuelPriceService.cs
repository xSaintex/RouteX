using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace RouteX.Services
{
    public interface IFuelPriceService
    {
        Task<decimal> GetCurrentFuelPriceAsync(string fuelType = "Regular");
        Task<Dictionary<string, decimal>> GetAllFuelPricesAsync();
        void ClearCache();
    }

    public class FuelPriceService : IFuelPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FuelPriceService> _logger;
        private readonly string _apiKey;

        // Cache fuel prices for 1 hour to avoid excessive API calls
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public FuelPriceService(IHttpClientFactory httpClientFactory, IMemoryCache cache, ILogger<FuelPriceService> logger, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _cache = cache;
            _logger = logger;
            _apiKey = configuration["FuelPrice:ApiKey"] ?? string.Empty;
        }

        public async Task<decimal> GetCurrentFuelPriceAsync(string fuelType = "Regular")
        {
            var cacheKey = $"FuelPrice_{fuelType}";
            
            if (_cache.TryGetValue(cacheKey, out decimal cachedPrice))
            {
                _logger.LogDebug("Returning cached fuel price for {FuelType}: {Price}", fuelType, cachedPrice);
                return cachedPrice;
            }

            try
            {
                var allPrices = await GetAllFuelPricesAsync();
                var price = allPrices.GetValueOrDefault(fuelType, 0m);
                
                // Cache the result
                _cache.Set(cacheKey, price, _cacheDuration);
                
                _logger.LogInformation("Retrieved fuel price for {FuelType}: {Price} PHP/L", fuelType, price);
                return price;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fuel price for {FuelType}", fuelType);
                return 0m;
            }
        }

        public async Task<Dictionary<string, decimal>> GetAllFuelPricesAsync()
        {
            var cacheKey = "AllFuelPrices";
            
            if (_cache.TryGetValue(cacheKey, out Dictionary<string, decimal> cachedPrices))
            {
                _logger.LogDebug("Returning cached fuel prices");
                return cachedPrices;
            }

            try
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    _logger.LogWarning("Fuel price API key not configured");
                    return GetDefaultPrices();
                }

                // Using Philippine fuel price API (you can replace with your preferred API)
                var apiUrl = $"https://api.fuelprice.ph/v1/current?apikey={_apiKey}";
                
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Fuel price API returned status: {StatusCode}", response.StatusCode);
                    return GetDefaultPrices();
                }

                var jsonContent = await response.Content.ReadAsStringAsync();
                var fuelData = JsonSerializer.Deserialize<FuelPriceResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (fuelData?.Prices == null)
                {
                    _logger.LogWarning("Invalid fuel price data received");
                    return GetDefaultPrices();
                }

                var prices = new Dictionary<string, decimal>
                {
                    ["Regular"] = fuelData.Prices.Regular ?? 0m,
                    ["Premium"] = fuelData.Prices.Premium ?? 0m,
                    ["Diesel"] = fuelData.Prices.Diesel ?? 0m,
                    ["Unleaded"] = fuelData.Prices.Unleaded ?? 0m,
                    ["Ethanol"] = fuelData.Prices.Ethanol ?? 0m
                };

                // Cache the result
                _cache.Set(cacheKey, prices, _cacheDuration);
                
                _logger.LogInformation("Successfully retrieved fuel prices: {Prices}", 
                    string.Join(", ", prices.Select(p => $"{p.Key}: PHP{p.Value:F2}")));
                
                return prices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching fuel prices from API");
                return GetDefaultPrices();
            }
        }

        public void ClearCache()
        {
            _cache.Remove("AllFuelPrices");
            _cache.Remove("FuelPrice_Regular");
            _cache.Remove("FuelPrice_Premium");
            _cache.Remove("FuelPrice_Diesel");
            _cache.Remove("FuelPrice_Unleaded");
            _cache.Remove("FuelPrice_Ethanol");
        }

        private Dictionary<string, decimal> GetDefaultPrices()
        {
            _logger.LogInformation("Using default fuel prices");
            return new Dictionary<string, decimal>
            {
                ["Regular"] = 58.50m,
                ["Premium"] = 62.75m,
                ["Diesel"] = 55.25m,
                ["Unleaded"] = 58.50m,
                ["Ethanol"] = 45.00m
            };
        }
    }

    // DTO for API response
    public class FuelPriceResponse
    {
        public FuelPrices? Prices { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Source { get; set; } = string.Empty;
    }

    public class FuelPrices
    {
        public decimal? Regular { get; set; }
        public decimal? Premium { get; set; }
        public decimal? Diesel { get; set; }
        public decimal? Unleaded { get; set; }
        public decimal? Ethanol { get; set; }
    }
}
