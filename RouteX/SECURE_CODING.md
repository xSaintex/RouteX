# Secure Coding: Secrets Handling (brief)

This file documents how hardcoded credentials were avoided and includes sample secure code you can apply to this project.

## How hardcoded credentials are avoided
- Remove real secrets from `appsettings.json` and replace with placeholders.
- Use `dotnet user-secrets` for local development.
- Use environment variables for CI/servers (`ConnectionStrings__DefaultConnection`, `TomTom__ApiKey`).
- Use a managed secrets provider in production (e.g. Azure Key Vault) and add it to configuration.
- Bind to typed options (`IOptions<T>`) and validate required values at startup.
- Never log secret values; log only warnings when missing.

## Sample secure code

Options class:

```csharp
public class TomTomOptions
{
    public string? ApiKey { get; set; }
}
```

Register and validate options in `Program.cs`:

```csharp
builder.Services.Configure<TomTomOptions>(builder.Configuration.GetSection("TomTom"));
builder.Services.AddOptions<TomTomOptions>()
       .Bind(builder.Configuration.GetSection("TomTom"))
       .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "TomTom.ApiKey is required");
```

Refactor `TomTomService` to use `IOptions<TomTomOptions>` instead of reading raw configuration:

```csharp
public class TomTomService : IRouteDistanceService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TomTomService> _logger;
    private readonly string _apiKey;

    public TomTomService(IHttpClientFactory httpClientFactory, IOptions<TomTomOptions> opts, ILogger<TomTomService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _apiKey = opts.Value.ApiKey ?? string.Empty;
    }

    // ... existing methods (use _apiKey, but never log its value)
}
```

Use `GetConnectionString("DefaultConnection")` as before, but provide the connection string via user-secrets or environment variables rather than committing it.

## Local developer commands

Initialize and set user-secrets (project folder):

```bash
cd RouteX
dotnet user-secrets init
dotnet user-secrets set "TomTom:ApiKey" "REPLACE_WITH_REAL_KEY"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=...;User Id=...;Password=...;"
```

Or set environment variables (PowerShell):

```powershell
$env:TomTom__ApiKey = "REPLACE_WITH_REAL_KEY"
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;User Id=...;Password=...;"
```

## Azure Key Vault (production)

Add Key Vault to `builder.Configuration` before `Build()`:

```csharp
var keyVaultUri = Environment.GetEnvironmentVariable("KEYVAULT_URI");
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}
```

## Screenshot locations (exact files + lines)
- `RouteX/appsettings.json` — FuelPrice ApiKey: lines 9–11
- `RouteX/appsettings.json` — TomTom ApiKey: lines 12–14
- `RouteX/appsettings.json` — ConnectionStrings (contains DB password): lines 15–16
- `RouteX/Program.cs` — DbContext registration & `GetConnectionString("DefaultConnection")`: lines 24–30
- `RouteX/Services/TomTomService.cs` — current constructor reading `configuration["TomTom:ApiKey"]`: lines 11–16
- `RouteX/Data/ApplicationDbContext.cs` — `_configuration.GetConnectionString("DefaultConnection")` fallback: lines 24–31

---
File created to provide quick guidance and ready-to-use code snippets for secure secret handling.
