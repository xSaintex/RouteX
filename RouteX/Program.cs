using Microsoft.AspNetCore.Identity;

using Microsoft.EntityFrameworkCore;

using RouteX.Data;

using RouteX.Models;

using RouteX.Services;



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

    options.Password.RequireDigit = false;

    options.Password.RequireLowercase = false;

    options.Password.RequireUppercase = false;

    options.Password.RequireNonAlphanumeric = false;

    options.Password.RequiredLength = 6;

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding database. Application will continue without seeding.");
        // Don't throw the exception - let the application start
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

