using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using RouteX.Data;

using RouteX.Models;

using RouteX.Services;

using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;



var builder = WebApplication.CreateBuilder(args);



// ================== SERVICES ==================



// Add DbContext (SQL Server) with improved error handling

builder.Services.AddDbContext<ApplicationDbContext>(options =>

    options.UseSqlServer(

        builder.Configuration.GetConnectionString("DefaultConnection"),

        sqlOptions => {

            sqlOptions.EnableRetryOnFailure(

                maxRetryCount: 3,

                maxRetryDelay: TimeSpan.FromSeconds(5),

                errorNumbersToAdd: null

            );

            sqlOptions.CommandTimeout(60);

        }

    )

);



// Add ASP.NET Core Identity

builder.Services.AddDefaultIdentity<IdentityUser>(options =>

{

    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;

    options.Password.RequireLowercase = true;

    options.Password.RequireUppercase = true;

    options.Password.RequireNonAlphanumeric = true;

    options.Password.RequiredLength = 8;

    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

    options.Lockout.MaxFailedAccessAttempts = 5;

    options.Lockout.AllowedForNewUsers = true;

})

.AddEntityFrameworkStores<ApplicationDbContext>();



builder.Services.ConfigureApplicationCookie(options =>

{

    options.LoginPath = "/Account/LoginPage";

});



// Add Session support for login
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});



// Add MVC

builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();

// Add Audit Service
builder.Services.AddScoped<IAuditService, AuditService>();

// Add Fuel Price Service
builder.Services.AddScoped<IFuelPriceService, FuelPriceService>();
builder.Services.AddScoped<IRouteDistanceService, TomTomService>();
builder.Services.AddScoped<ITextFormattingService, TextFormattingService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddMemoryCache(); // Required for fuel price caching
builder.Services.AddHttpClient(); // Required for FuelPriceService

// Add Rate Limiting (Issue 8)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});



var app = builder.Build();



// ================== DATABASE SEEDING ==================

// Seed default admin user with Identity (with better error handling)

using (var scope = app.Services.CreateScope())

{

    var services = scope.ServiceProvider;

    try

    {

        var context = services.GetRequiredService<ApplicationDbContext>();

        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();



        // Wait a moment for database to be ready

        await Task.Delay(3000);



        // Ensure database is created
        context.Database.EnsureCreated();
                
    }
    catch (Exception ex)
    {

        // Log but don't fail - Identity user is the important part
        var logger = userManager.Logger;
        logger.LogWarning(ex, "Could not update custom Users table for {Email}", email);

    }

}


// ================== MIDDLEWARE ==================



if (!app.Environment.IsDevelopment())

{

    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();

}



app.UseHttpsRedirection();

app.UseStaticFiles();



app.UseRouting();

// Rate limiting middleware (after UseRouting, before UseAuthentication)
app.UseRateLimiter();

// Add Session middleware

app.UseSession();



// Identity authentication and authorization

app.UseAuthentication();

app.UseAuthorization();



// ================== ROUTES ==================



app.MapControllerRoute(

    name: "default",

    pattern: "{controller=Account}/{action=LoginPage}/{id?}"

);



// Required for Identity UI

app.MapRazorPages();



app.Run();

